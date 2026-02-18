using ImageBatchGenerator.Application.Interfaces;
using ImageBatchGenerator.Application.Orchestration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImageBatchGenerator.Web.BackgroundServices;

/// <summary>
/// ジョブキュー監視バックグラウンドサービス
/// IHostedServiceとして起動し、IJobQueueを常時監視してジョブを実行する
/// 最大同時実行ジョブ数はappsettingsで制御する
/// </summary>
public class JobQueueWorker : BackgroundService
{
    private readonly IJobQueue _jobQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobQueueWorker> _logger;

    // TODO: appsettingsから注入（Phase 3）
    private readonly int _maxConcurrentJobs = 2;

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
        // TODO: Phase 3で実装
        // SemaphoreSlimで同時実行数を制限しながらジョブを処理する
        //
        // var semaphore = new SemaphoreSlim(_maxConcurrentJobs);
        // while (!stoppingToken.IsCancellationRequested)
        // {
        //     var jobId = await _jobQueue.DequeueAsync(stoppingToken);
        //     if (jobId is null) continue;
        //
        //     await semaphore.WaitAsync(stoppingToken);
        //     _ = Task.Run(async () =>
        //     {
        //         try
        //         {
        //             using var scope = _scopeFactory.CreateScope();
        //             var orchestrator = scope.ServiceProvider.GetRequiredService<JobOrchestrator>();
        //             await orchestrator.ExecuteJobAsync(jobId.Value, stoppingToken);
        //         }
        //         finally { semaphore.Release(); }
        //     }, stoppingToken);
        // }
        throw new NotImplementedException();
    }
}
