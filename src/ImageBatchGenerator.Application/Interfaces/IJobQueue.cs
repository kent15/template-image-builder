namespace ImageBatchGenerator.Application.Interfaces;

/// <summary>
/// ジョブキューインターフェース
/// MVP実装: InMemoryJobQueue（System.Threading.Channels）
/// 将来拡張: RabbitMqJobQueue（差し替えのみで対応可能）
/// </summary>
public interface IJobQueue
{
    // TODO: メソッド実装（Phase 3）

    /// <summary>ジョブIDをキューに追加する</summary>
    Task EnqueueAsync(Guid jobId, CancellationToken ct = default);

    /// <summary>
    /// キューからジョブIDを取り出す（待機あり）
    /// キューが空の場合はnullを返す
    /// </summary>
    Task<Guid?> DequeueAsync(CancellationToken ct = default);

    /// <summary>現在のキュー長を取得する</summary>
    Task<int> GetQueueLengthAsync();
}
