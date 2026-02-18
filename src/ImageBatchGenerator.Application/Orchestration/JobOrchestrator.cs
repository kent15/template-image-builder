using ImageBatchGenerator.Application.Interfaces;
using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.Interfaces;
using ImageBatchGenerator.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ImageBatchGenerator.Application.Orchestration;

/// <summary>
/// ジョブオーケストレーター
/// JobQueueWorker から呼び出され、1ジョブの処理フロー全体を統括する
/// アイテムの並列処理は BatchCoordinator に委譲する
/// </summary>
public class JobOrchestrator
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobItemRepository _jobItemRepository;
    private readonly IJobCancellationRegistry _cancellationRegistry;
    private readonly BatchCoordinator _batchCoordinator;
    private readonly ILogger<JobOrchestrator> _logger;

    public JobOrchestrator(
        IJobRepository jobRepository,
        IJobItemRepository jobItemRepository,
        IJobCancellationRegistry cancellationRegistry,
        BatchCoordinator batchCoordinator,
        ILogger<JobOrchestrator> logger)
    {
        _jobRepository = jobRepository;
        _jobItemRepository = jobItemRepository;
        _cancellationRegistry = cancellationRegistry;
        _batchCoordinator = batchCoordinator;
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

        job.Start();
        await _jobRepository.UpdateAsync(job);
        _logger.LogInformation("Job {JobId} started. TotalCount={Total}", jobId, job.TotalCount);

        var pendingItems = await _jobItemRepository
            .GetByJobIdAndStatusAsync(jobId, JobItemStatus.Pending);

        try
        {
            await _batchCoordinator.ProcessAsync(job, pendingItems, ct);
        }
        catch (OperationCanceledException)
        {
            // キャンセルは完了判定で処理する
        }

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
    /// チェックポイントから処理を再開する（Resume 対応）
    /// </summary>
    public async Task ResumeJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job is null) return;

        var pendingItems = await _jobItemRepository
            .GetPendingAfterIndexAsync(jobId, job.LastProcessedIndex);

        try
        {
            await _batchCoordinator.ProcessAsync(job, pendingItems, ct);
        }
        catch (OperationCanceledException) { }

        if (!ct.IsCancellationRequested)
            job.Complete();

        await _jobRepository.UpdateAsync(job);
    }
}
