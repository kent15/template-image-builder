namespace ImageBatchGenerator.Application.DTOs;

/// <summary>
/// リアルタイム進捗通知用DTO（SignalRで送信）
/// </summary>
public record JobProgressDto
{
    public Guid JobId { get; init; }
    public int ProcessedCount { get; init; }
    public int TotalCount { get; init; }
    public int SuccessCount { get; init; }
    public int WarningCount { get; init; }
    public int ErrorCount { get; init; }

    /// <summary>進捗率（0.0〜1.0）</summary>
    public double ProgressRate { get; init; }

    /// <summary>処理速度（件/秒）</summary>
    public double? ItemsPerSecond { get; init; }

    /// <summary>推定残り時間（秒）</summary>
    public double? EstimatedRemainingSeconds { get; init; }

    // TODO: マッピング実装（Phase 3）
}
