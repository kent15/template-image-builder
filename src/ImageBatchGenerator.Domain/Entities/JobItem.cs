using ImageBatchGenerator.Domain.ValueObjects;

namespace ImageBatchGenerator.Domain.Entities;

/// <summary>
/// ジョブ明細（CSV1行 = 1件の画像生成単位）
/// </summary>
public class JobItem
{
    public Guid Id { get; private set; }
    public Guid JobId { get; private set; }

    /// <summary>CSVの行インデックス（0始まり）</summary>
    public int RowIndex { get; private set; }

    public JobItemStatus Status { get; private set; }

    /// <summary>CSV行データのスナップショット（再実行用）</summary>
    public string? InputDataJson { get; private set; }

    public string? WarningMessage { get; private set; }
    public int RetryCount { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    /// <summary>処理時間（ミリ秒）</summary>
    public int? ProcessingMs { get; private set; }

    public Job? Job { get; private set; }
    public GeneratedImage? GeneratedImage { get; private set; }

    private JobItem() { }

    /// <summary>ジョブ明細を新規作成する</summary>
    public static JobItem Create(Guid jobId, int rowIndex, string? inputDataJson = null)
    {
        return new JobItem
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            RowIndex = rowIndex,
            Status = JobItemStatus.Pending,
            InputDataJson = inputDataJson,
        };
    }

    /// <summary>処理を開始する</summary>
    public void MarkRunning()
    {
        Status = JobItemStatus.Running;
    }

    /// <summary>成功完了にする</summary>
    public void MarkSuccess(int processingMs)
    {
        Status = JobItemStatus.Success;
        ProcessedAt = DateTimeOffset.UtcNow;
        ProcessingMs = processingMs;
    }

    /// <summary>警告付き完了にする</summary>
    public void MarkWarning(string warningMessage, int processingMs)
    {
        Status = JobItemStatus.Warning;
        WarningMessage = warningMessage;
        ProcessedAt = DateTimeOffset.UtcNow;
        ProcessingMs = processingMs;
    }

    /// <summary>エラー状態にする</summary>
    public void MarkError(string errorMessage)
    {
        Status = JobItemStatus.Error;
        WarningMessage = errorMessage;
        RetryCount++;
    }

    /// <summary>エラー再実行のためにPendingに戻す</summary>
    public void ResetForRetry()
    {
        Status = JobItemStatus.Pending;
        WarningMessage = null;
    }
}
