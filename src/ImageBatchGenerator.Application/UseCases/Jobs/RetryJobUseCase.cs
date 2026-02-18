using ImageBatchGenerator.Application.Interfaces;
using ImageBatchGenerator.Domain.Interfaces;
using ImageBatchGenerator.Domain.ValueObjects;

namespace ImageBatchGenerator.Application.UseCases.Jobs;

/// <summary>
/// ジョブ再実行ユースケース
/// エラー状態の JobItem のみを Pending に戻してキューに再投入する
/// </summary>
public class RetryJobUseCase
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobItemRepository _jobItemRepository;
    private readonly IJobQueue _jobQueue;

    public RetryJobUseCase(
        IJobRepository jobRepository,
        IJobItemRepository jobItemRepository,
        IJobQueue jobQueue)
    {
        _jobRepository = jobRepository;
        _jobItemRepository = jobItemRepository;
        _jobQueue = jobQueue;
    }

    /// <summary>
    /// エラー分のみ Pending に戻してキューに再投入する
    /// </summary>
    public async Task ExecuteAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, ct)
            ?? throw new InvalidOperationException($"Job not found: {jobId}");

        var errorItems = await _jobItemRepository
            .GetByJobIdAndStatusAsync(jobId, JobItemStatus.Error, ct);

        foreach (var item in errorItems)
            item.ResetForRetry();

        await _jobItemRepository.UpdateRangeAsync(errorItems, ct);

        job.ResetForRetry(); // ErrorCount リセット + Enqueue()
        await _jobRepository.UpdateAsync(job, ct);
        await _jobQueue.EnqueueAsync(jobId, ct);
    }
}
