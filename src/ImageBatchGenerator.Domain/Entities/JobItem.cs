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

    // TODO: ドメインロジック実装（Phase 3）

    /// <summary>処理を開始する</summary>
    public void MarkRunning()
    {
        // TODO: Statusを実行中に変更する
        throw new NotImplementedException();
    }

    /// <summary>成功完了にする</summary>
    public void MarkSuccess(int processingMs)
    {
        // TODO: Statusを成功に変更し、ProcessedAt・ProcessingMsを設定する
        throw new NotImplementedException();
    }

    /// <summary>警告付き完了にする</summary>
    public void MarkWarning(string warningMessage, int processingMs)
    {
        // TODO: Statusを警告に変更し、WarningMessageを設定する
        throw new NotImplementedException();
    }

    /// <summary>エラー状態にする</summary>
    public void MarkError(string errorMessage)
    {
        // TODO: Statusをエラーに変更し、RetryCountを加算する
        throw new NotImplementedException();
    }

    /// <summary>エラー再実行のためにPendingに戻す</summary>
    public void ResetForRetry()
    {
        // TODO: StatusをPendingに戻す
        throw new NotImplementedException();
    }
}
