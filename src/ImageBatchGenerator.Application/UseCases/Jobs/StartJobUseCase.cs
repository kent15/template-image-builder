using ImageBatchGenerator.Application.Interfaces;
using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Application.UseCases.Jobs;

/// <summary>
/// ジョブ実行開始ユースケース
/// ジョブを Queued 状態にしてインメモリキューに追加する
/// </summary>
public class StartJobUseCase
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobQueue _jobQueue;

    public StartJobUseCase(IJobRepository jobRepository, IJobQueue jobQueue)
    {
        _jobRepository = jobRepository;
        _jobQueue = jobQueue;
    }

    /// <summary>
    /// ジョブをキューに投入する
    /// </summary>
    public async Task ExecuteAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, ct)
            ?? throw new InvalidOperationException($"Job not found: {jobId}");

        job.Enqueue();
        await _jobRepository.UpdateAsync(job, ct);
        await _jobQueue.EnqueueAsync(jobId, ct);
    }
}
