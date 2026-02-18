namespace ImageBatchGenerator.Domain.ValueObjects;

/// <summary>
/// ジョブのステータス値オブジェクト
/// Created → Queued → Running → Completed / CompletedWithWarning / Failed / Cancelled
/// </summary>
public enum JobStatus
{
    /// <summary>作成済み（未キュー）</summary>
    Created,

    /// <summary>キュー待機中</summary>
    Queued,

    /// <summary>実行中</summary>
    Running,

    /// <summary>正常完了</summary>
    Completed,

    /// <summary>警告付き完了（一部エラーあり）</summary>
    CompletedWithWarning,

    /// <summary>失敗（致命的エラーによるジョブ停止）</summary>
    Failed,

    /// <summary>ユーザーによるキャンセル</summary>
    Cancelled,
}
