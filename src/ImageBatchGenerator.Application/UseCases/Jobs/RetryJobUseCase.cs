using ImageBatchGenerator.Application.Interfaces;
using ImageBatchGenerator.Domain.Interfaces;
using ImageBatchGenerator.Domain.ValueObjects;

namespace ImageBatchGenerator.Application.UseCases.Jobs;

/// <summary>
/// ジョブ再実行ユースケース
/// エラー状態のJobItemのみをPendingに戻してキューに再投入する
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
    /// エラー分のみ再実行する
    /// </summary>
    public async Task ExecuteAsync(Guid jobId, CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        // 1. ジョブ取得・存在確認
        // 2. StatusがFailed/CompletedWithWarningであることを確認
        // 3. Status = Error のJobItemをすべてPendingに戻す
        // 4. Job.StatusをRunning相当に変更・ErrorCountをリセット
        // 5. DBを更新
        // 6. IJobQueueにjobIdを追加
        throw new NotImplementedException();
    }
}
