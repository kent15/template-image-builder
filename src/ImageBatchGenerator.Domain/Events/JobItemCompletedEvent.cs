using ImageBatchGenerator.Domain.ValueObjects;

namespace ImageBatchGenerator.Domain.Events;

/// <summary>
/// ジョブ明細1件完了ドメインイベント（SignalR進捗通知のトリガー）
/// </summary>
public record JobItemCompletedEvent
{
    public Guid JobId { get; init; }
    public Guid JobItemId { get; init; }
    public int RowIndex { get; init; }
    public JobItemStatus Status { get; init; }
    public int ProcessedCount { get; init; }
    public int TotalCount { get; init; }
    public DateTimeOffset OccurredAt { get; init; }

    // TODO: イベントハンドラ実装（Phase 3）
}
