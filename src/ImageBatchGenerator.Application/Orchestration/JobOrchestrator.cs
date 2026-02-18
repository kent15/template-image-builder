using ImageBatchGenerator.Application.Interfaces;
using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.Interfaces;
using ImageBatchGenerator.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ImageBatchGenerator.Application.Orchestration;

/// <summary>
/// ジョブオーケストレーター（Phase 1: 順次処理）
/// JobQueueWorker から呼び出され、1ジョブの処理フロー全体を統括する
/// Phase 2以降: BatchCoordinator に並列処理を委譲する
/// </summary>
public class JobOrchestrator
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobItemRepository _jobItemRepository;
    private readonly IJobCancellationRegistry _cancellationRegistry;
    private readonly ILogger<JobOrchestrator> _logger;

    public JobOrchestrator(
        IJobRepository jobRepository,
        IJobItemRepository jobItemRepository,
        IJobCancellationRegistry cancellationRegistry,
        ILogger<JobOrchestrator> logger)
    {
        _jobRepository = jobRepository;
        _jobItemRepository = jobItemRepository;
        _cancellationRegistry = cancellationRegistry;
        _logger = logger;
    }

    /// <summary>
    /// 1ジョブの処理を開始から完了まで統括する
    /// ホストの stoppingToken とジョブ固有の CancellationToken をリンクして使用する
    /// </summary>
    public async Task ExecuteJobAsync(Guid jobId, CancellationToken hostCt = default)
    {
        var jobCt = _cancellationRegistry.Register(jobId);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(hostCt, jobCt);

        try
        {
            await RunAsync(jobId, linked.Token);
        }
        finally
        {
            _cancellationRegistry.Unregister(jobId);
        }
    }

    private async Task RunAsync(Guid jobId, CancellationToken ct)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job is null)
        {
            _logger.LogWarning("Job {JobId} not found. Skipping.", jobId);
            return;
        }

        // キャンセル済みや予期しない状態は処理しない
        if (job.Status != JobStatus.Queued)
        {
            _logger.LogWarning("Job {JobId} is not Queued (current: {Status}). Skipping.", jobId, job.Status);
            return;
        }

        // Running へ遷移
        job.Start();
        await _jobRepository.UpdateAsync(job);
        _logger.LogInformation("Job {JobId} started. TotalCount={Total}", jobId, job.TotalCount);

        // Pending の JobItem を順次処理
        var pendingItems = await _jobItemRepository
            .GetByJobIdAndStatusAsync(jobId, JobItemStatus.Pending);

        foreach (var item in pendingItems)
        {
            if (ct.IsCancellationRequested)
                break;

            await ProcessItemAsync(job, item, ct);
        }

        // 完了 or キャンセル確定
        if (ct.IsCancellationRequested)
        {
            job.Cancel();
            _logger.LogInformation("Job {JobId} cancelled. Processed={Processed}/{Total}",
                jobId, job.ProcessedCount, job.TotalCount);
        }
        else
        {
            job.Complete();
            _logger.LogInformation("Job {JobId} completed. S={S} W={W} E={E}",
                jobId, job.SuccessCount, job.WarningCount, job.ErrorCount);
        }

        await _jobRepository.UpdateAsync(job);
    }

    /// <summary>
    /// 1件の JobItem を処理する（Phase 1: モック。Phase 2以降で実画像生成に差し替え）
    /// </summary>
    private async Task ProcessItemAsync(Job job, JobItem item, CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        item.MarkRunning();
        await _jobItemRepository.UpdateAsync(item);

        // Phase 2以降: IImageProcessor.GenerateAsync() + IStorageService.SaveAsync()
        await Task.Yield(); // 非同期継続を確保するプレースホルダー

        sw.Stop();
        item.MarkSuccess((int)sw.ElapsedMilliseconds);
        job.IncrementProgress(success: true, warning: false, error: false);
        job.UpdateCheckpoint(item.RowIndex);

        await _jobItemRepository.UpdateAsync(item);
        await _jobRepository.UpdateAsync(job);
    }

    /// <summary>
    /// チェックポイントから処理を再開する（Resume 対応）
    /// </summary>
    public async Task ResumeJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job is null) return;

        var pendingItems = await _jobItemRepository
            .GetPendingAfterIndexAsync(jobId, job.LastProcessedIndex);

        foreach (var item in pendingItems)
        {
            if (ct.IsCancellationRequested)
                break;

            await ProcessItemAsync(job, item, ct);
        }

        if (!ct.IsCancellationRequested)
            job.Complete();

        await _jobRepository.UpdateAsync(job);
    }
}
