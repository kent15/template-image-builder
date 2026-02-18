using ImageBatchGenerator.Application.Interfaces;
using ImageBatchGenerator.Application.Orchestration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImageBatchGenerator.Web.BackgroundServices;

/// <summary>
/// ジョブキュー監視バックグラウンドサービス（Phase 1: 順次処理）
/// IJobQueue を常時監視し、ジョブを 1 件ずつ順番に処理する
/// Phase 3以降: SemaphoreSlim で同時実行数を制御した並列処理に変更する
/// </summary>
public class JobQueueWorker : BackgroundService
{
    private readonly IJobQueue _jobQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobQueueWorker> _logger;

    public JobQueueWorker(
        IJobQueue jobQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<JobQueueWorker> logger)
    {
        _jobQueue = jobQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
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

            _logger.LogInformation("Dequeued job {JobId}.", jobId.Value);

            try
            {
                // Scoped サービス（JobOrchestrator）をジョブ単位のスコープで解決する
                using var scope = _scopeFactory.CreateScope();
                var orchestrator = scope.ServiceProvider
                    .GetRequiredService<JobOrchestrator>();

                await orchestrator.ExecuteJobAsync(jobId.Value, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Job {JobId} processing was cancelled.", jobId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Job {JobId} failed unexpectedly.", jobId.Value);
            }
        }

        _logger.LogInformation("JobQueueWorker stopped.");
    }
}
