using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;
using ImageBatchGenerator.Application.DTOs;
using ImageBatchGenerator.Application.Interfaces;
using ImageBatchGenerator.Application.Options;
using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImageBatchGenerator.Application.Orchestration;

/// <summary>
/// バッチ並列処理コーディネーター
/// System.Threading.Channels の Producer-Consumer パターンで N 並列実行する
/// SemaphoreSlim(1,1) でジョブ状態更新をシリアライズしてスレッドセーフを保証する
/// </summary>
public class BatchCoordinator
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly IJobRepository _jobRepository;
    private readonly IJobItemRepository _jobItemRepository;
    private readonly IImageProcessor _imageProcessor;
    private readonly IStorageService _storageService;
    private readonly IProgressNotifier _progressNotifier;
    private readonly ITemplateRepository _templateRepository;
    private readonly ILogger<BatchCoordinator> _logger;
    private readonly int _maxDegreeOfParallelism;

    public BatchCoordinator(
        IJobRepository jobRepository,
        IJobItemRepository jobItemRepository,
        IImageProcessor imageProcessor,
        IStorageService storageService,
        IProgressNotifier progressNotifier,
        ITemplateRepository templateRepository,
        ILogger<BatchCoordinator> logger,
        IOptions<BatchSettings> options)
    {
        _jobRepository = jobRepository;
        _jobItemRepository = jobItemRepository;
        _imageProcessor = imageProcessor;
        _storageService = storageService;
        _progressNotifier = progressNotifier;
        _templateRepository = templateRepository;
        _logger = logger;
        _maxDegreeOfParallelism = options.Value.MaxDegreeOfParallelism;
    }

    /// <summary>
    /// JobItem リストを並列処理する
    /// キャンセル時は OperationCanceledException を呼び出し元に伝播する
    /// </summary>
    public async Task ProcessAsync(
        Job job,
        IReadOnlyList<JobItem> items,
        CancellationToken ct = default)
    {
        if (items.Count == 0) return;

        // テンプレートをジョブ開始前に一度だけロード
        var template = await _templateRepository.GetByIdAsync(job.TemplateId, ct);
        if (template is null)
            _logger.LogWarning("Job {JobId}: Template {TemplateId} not found. Items will be marked as error.", job.Id, job.TemplateId);

        var outputSettings = OutputSettings.Parse(job.OutputSettingsJson);

        // Channel capacity = parallelism * 2 でメモリ使用量を抑制
        var channel = Channel.CreateBounded<JobItem>(new BoundedChannelOptions(_maxDegreeOfParallelism * 2)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = true,
            SingleReader = false,
        });

        // ジョブ状態更新（IncrementProgress, UpdateCheckpoint, UpdateAsync）をシリアライズ
        using var jobSemaphore = new SemaphoreSlim(1, 1);

        // N 並列コンシューマーを起動
        var consumers = Enumerable
            .Range(0, _maxDegreeOfParallelism)
            .Select(_ => ConsumeAsync(job, template, outputSettings, channel.Reader, jobSemaphore, ct))
            .ToArray();

        // プロデューサー: アイテムをチャネルに書き込む
        try
        {
            foreach (var item in items)
            {
                ct.ThrowIfCancellationRequested();
                await channel.Writer.WriteAsync(item, ct);
            }
        }
        catch
        {
            // キャンセルまたは例外発生時はチャネルを完了させてコンシューマーを終了させる
            channel.Writer.TryComplete();
            await Task.WhenAll(consumers);
            throw;
        }

        // 正常終了: チャネルを完了させてコンシューマーが残りアイテムを処理するのを待つ
        channel.Writer.Complete();
        await Task.WhenAll(consumers);
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Consumer
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private async Task ConsumeAsync(
        Job job,
        Template? template,
        OutputSettings outputSettings,
        ChannelReader<JobItem> reader,
        SemaphoreSlim jobSemaphore,
        CancellationToken ct)
    {
        try
        {
            await foreach (var item in reader.ReadAllAsync(ct))
            {
                await ProcessItemSafelyAsync(job, template, outputSettings, item, jobSemaphore, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // キャンセルは正常系 — コンシューマーを静かに終了する
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Item Processing
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private async Task ProcessItemSafelyAsync(
        Job job,
        Template? template,
        OutputSettings outputSettings,
        JobItem item,
        SemaphoreSlim jobSemaphore,
        CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        item.MarkRunning();
        await _jobItemRepository.UpdateAsync(item);

        bool success = false;
        string? errorMessage = null;

        try
        {
            if (template is null)
                throw new InvalidOperationException($"Template {job.TemplateId} not found.");

            var inputData = ParseInputData(item.InputDataJson);

            var imageBytes = await _imageProcessor.GenerateAsync(
                template, inputData, outputSettings.GetOutputFormat(), outputSettings.Quality, ct);

            using var ms = new MemoryStream(imageBytes);
            var extension = outputSettings.Format.ToLowerInvariant();
            var fileName = $"{item.RowIndex:D6}.{extension}";
            await _storageService.SaveAsync(ms, fileName, job.Id.ToString(), ct);

            success = true;
        }
        catch (OperationCanceledException)
        {
            throw; // コンシューマーに伝播してループを抜ける
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            _logger.LogError(ex, "Job {JobId} Item RowIndex={RowIndex} failed.", job.Id, item.RowIndex);
        }
        finally
        {
            sw.Stop();
        }

        // ジョブ状態更新をシリアライズ（SemaphoreSlim で排他制御）
        // OperationCanceledException 発生時はここに到達しない
        await jobSemaphore.WaitAsync(CancellationToken.None);
        try
        {
            if (success)
            {
                item.MarkSuccess((int)sw.ElapsedMilliseconds);
                job.IncrementProgress(success: true, warning: false, error: false);
            }
            else
            {
                item.MarkError(errorMessage!);
                job.IncrementProgress(success: false, warning: false, error: true);
            }

            job.UpdateCheckpoint(item.RowIndex);
            await _jobItemRepository.UpdateAsync(item);
            await _jobRepository.UpdateAsync(job);
        }
        finally
        {
            jobSemaphore.Release();
        }

        // 進捗通知（非クリティカル — 例外は握りつぶす）
        try
        {
            var progressDto = new JobProgressDto
            {
                JobId = job.Id,
                ProcessedCount = job.ProcessedCount,
                TotalCount = job.TotalCount,
                SuccessCount = job.SuccessCount,
                WarningCount = job.WarningCount,
                ErrorCount = job.ErrorCount,
                ProgressRate = job.TotalCount > 0
                    ? (double)job.ProcessedCount / job.TotalCount
                    : 0,
            };
            await _progressNotifier.NotifyProgressAsync(progressDto, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Progress notification failed for job {JobId}.", job.Id);
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Helpers
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private static IReadOnlyDictionary<string, string> ParseInputData(string? inputDataJson)
    {
        if (string.IsNullOrWhiteSpace(inputDataJson))
            return new Dictionary<string, string>();

        return JsonSerializer.Deserialize<Dictionary<string, string>>(inputDataJson, JsonOpts)
            ?? new Dictionary<string, string>();
    }
}
