using ImageBatchGenerator.Application.Interfaces;
using ImageBatchGenerator.Application.Options;
using ImageBatchGenerator.Application.Orchestration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImageBatchGenerator.Web.BackgroundServices;

/// <summary>
/// ジョブキュー監視バックグラウンドサービス
/// SemaphoreSlim(MaxConcurrentJobs) で同時実行数を制御し、並列ジョブ処理を行う
/// 各ジョブは独立した DI スコープ内で JobOrchestrator → BatchCoordinator へ委譲される
/// </summary>
public class JobQueueWorker : BackgroundService
{
    private readonly IJobQueue _jobQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobQueueWorker> _logger;
    private readonly SemaphoreSlim _concurrencySlot;

    public JobQueueWorker(
        IJobQueue jobQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<JobQueueWorker> logger,
        IOptions<BatchSettings> options)
    {
        _jobQueue = jobQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _concurrencySlot = new SemaphoreSlim(
            options.Value.MaxConcurrentJobs,
            options.Value.MaxConcurrentJobs);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JobQueueWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            Guid? jobId;
            try
            {
                jobId = await _jobQueue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (jobId is null) continue;

            _logger.LogInformation("Dequeued job {JobId}. Waiting for concurrency slot.", jobId.Value);

            try
            {
                await _concurrencySlot.WaitAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            var capturedJobId = jobId.Value;

            // ジョブ処理を fire-and-forget で並列実行。完了時にセマフォを解放する
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var orchestrator = scope.ServiceProvider
                        .GetRequiredService<JobOrchestrator>();

                    await orchestrator.ExecuteJobAsync(capturedJobId, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Job {JobId} processing was cancelled.", capturedJobId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Job {JobId} failed unexpectedly.", capturedJobId);
                }
                finally
                {
                    _concurrencySlot.Release();
                }
            }, stoppingToken);
        }

        _logger.LogInformation("JobQueueWorker stopped.");
    }

    public override void Dispose()
    {
        _concurrencySlot.Dispose();
        base.Dispose();
    }
}
