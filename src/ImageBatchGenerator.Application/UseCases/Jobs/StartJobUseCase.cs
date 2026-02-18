using ImageBatchGenerator.Application.Interfaces;
using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Application.UseCases.Jobs;

/// <summary>
/// ジョブ実行開始ユースケース
/// ジョブをQueuedにしてキューに追加する
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
        // TODO: Phase 3で実装
        // 1. ジョブ取得・存在確認
        // 2. StatusがCreatedであることを確認
        // 3. Job.Enqueue()を呼び出してステータス変更
        // 4. DBを更新
        // 5. IJobQueueにjobIdを追加
        throw new NotImplementedException();
    }
}
