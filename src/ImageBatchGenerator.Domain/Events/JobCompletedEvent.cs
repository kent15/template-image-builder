using ImageBatchGenerator.Domain.ValueObjects;

namespace ImageBatchGenerator.Domain.Events;

/// <summary>
/// ジョブ全体完了ドメインイベント
/// </summary>
public record JobCompletedEvent
{
    public Guid JobId { get; init; }
    public string JobName { get; init; } = string.Empty;
    public JobStatus FinalStatus { get; init; }
    public int SuccessCount { get; init; }
    public int WarningCount { get; init; }
    public int ErrorCount { get; init; }
    public DateTimeOffset OccurredAt { get; init; }

    // TODO: イベントハンドラ実装（Phase 3）
}
