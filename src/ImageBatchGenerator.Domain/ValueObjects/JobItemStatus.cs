namespace ImageBatchGenerator.Domain.ValueObjects;

/// <summary>
/// ジョブ明細のステータス値オブジェクト
/// Pending → Running → Success / Warning / Error / Skipped
/// </summary>
public enum JobItemStatus
{
    /// <summary>処理待ち</summary>
    Pending,

    /// <summary>処理中</summary>
    Running,

    /// <summary>正常完了</summary>
    Success,

    /// <summary>警告付き完了（自動フォールバック適用）</summary>
    Warning,

    /// <summary>エラー（個別スキップ）</summary>
    Error,

    /// <summary>スキップ済み</summary>
    Skipped,
}
