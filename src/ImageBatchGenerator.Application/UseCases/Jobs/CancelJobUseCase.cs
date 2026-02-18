using ImageBatchGenerator.Application.Interfaces;
using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Application.UseCases.Jobs;

/// <summary>
/// ジョブキャンセルユースケース
/// 実行中ジョブには IJobCancellationRegistry 経由でキャンセルを通知し、
/// リポジトリのステータスも即時 Cancelled に更新する
/// </summary>
public class CancelJobUseCase
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobCancellationRegistry _cancellationRegistry;

    public CancelJobUseCase(
        IJobRepository jobRepository,
        IJobCancellationRegistry cancellationRegistry)
    {
        _jobRepository = jobRepository;
        _cancellationRegistry = cancellationRegistry;
    }

    /// <summary>
    /// ジョブをキャンセルする
    /// - Queued 状態: ステータスを即時 Cancelled に変更（キュー消費前に処理をスキップさせる）
    /// - Running 状態: CancellationToken をキャンセルし、Orchestrator に停止を通知する
    /// </summary>
    public async Task ExecuteAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, ct)
            ?? throw new InvalidOperationException($"Job not found: {jobId}");

        // 実行中の場合は Orchestrator の CancellationToken にシグナルを送る
        _cancellationRegistry.Cancel(jobId);

        // UI 即時反映のためリポジトリも更新する
        job.Cancel();
        await _jobRepository.UpdateAsync(job, ct);
    }
}
