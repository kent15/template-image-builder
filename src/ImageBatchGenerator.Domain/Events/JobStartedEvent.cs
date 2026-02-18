namespace ImageBatchGenerator.Domain.Events;

/// <summary>
/// ジョブ実行開始ドメインイベント
/// </summary>
public record JobStartedEvent
{
    public Guid JobId { get; init; }
    public string JobName { get; init; } = string.Empty;
    public int TotalCount { get; init; }
    public DateTimeOffset OccurredAt { get; init; }

    // TODO: イベントハンドラ実装（Phase 3）
}
