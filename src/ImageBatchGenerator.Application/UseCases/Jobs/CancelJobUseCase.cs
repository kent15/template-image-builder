using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Application.UseCases.Jobs;

/// <summary>
/// ジョブキャンセルユースケース
/// インメモリ実装: ステータスを Cancelled に更新する
/// Phase 3以降: CancellationTokenRegistry で実行中ワーカーを停止する
/// </summary>
public class CancelJobUseCase
{
    private readonly IJobRepository _jobRepository;

    public CancelJobUseCase(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    /// <summary>
    /// ジョブをキャンセルする
    /// </summary>
    public async Task ExecuteAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, ct)
            ?? throw new InvalidOperationException($"Job not found: {jobId}");

        job.Cancel();
        await _jobRepository.UpdateAsync(job, ct);
    }
}
