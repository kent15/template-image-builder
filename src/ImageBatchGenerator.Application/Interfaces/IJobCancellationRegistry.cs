namespace ImageBatchGenerator.Application.Interfaces;

/// <summary>
/// 実行中ジョブのキャンセルトークン管理インターフェース
/// JobOrchestrator が Register/Unregister し、CancelJobUseCase が Cancel を呼ぶ
/// </summary>
public interface IJobCancellationRegistry
{
    /// <summary>ジョブを登録してキャンセルトークンを返す</summary>
    CancellationToken Register(Guid jobId);

    /// <summary>ジョブのキャンセルを要求する（未登録なら何もしない）</summary>
    void Cancel(Guid jobId);

    /// <summary>ジョブの登録を解除して CancellationTokenSource を破棄する</summary>
    void Unregister(Guid jobId);
}
