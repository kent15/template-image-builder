using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Application.UseCases.Jobs;

/// <summary>
/// ジョブキャンセルユースケース
/// 実行中ジョブにCancellationTokenを通知して停止する
/// </summary>
public class CancelJobUseCase
{
    private readonly IJobRepository _jobRepository;

    public CancelJobUseCase(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    /// <summary>
    /// 実行中ジョブをキャンセルする
    /// </summary>
    public async Task ExecuteAsync(Guid jobId, CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        // 1. ジョブ取得・存在確認
        // 2. StatusがRunning/Queuedであることを確認
        // 3. CancellationTokenRegistryでCTSをキャンセル
        // 4. Job.Cancel()を呼び出してステータス変更
        // 5. DBを更新
        throw new NotImplementedException();
    }
}
