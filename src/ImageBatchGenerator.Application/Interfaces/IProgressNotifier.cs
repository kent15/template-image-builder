using ImageBatchGenerator.Application.DTOs;

namespace ImageBatchGenerator.Application.Interfaces;

/// <summary>
/// リアルタイム進捗通知インターフェース（SignalRで実装）
/// </summary>
public interface IProgressNotifier
{
    // TODO: メソッド実装（Phase 3）

    /// <summary>ジョブの進捗をクライアントにプッシュ通知する</summary>
    Task NotifyProgressAsync(JobProgressDto progress, CancellationToken ct = default);

    /// <summary>ジョブ完了をクライアントに通知する</summary>
    Task NotifyCompletedAsync(Guid jobId, CancellationToken ct = default);

    /// <summary>ジョブエラーをクライアントに通知する</summary>
    Task NotifyErrorAsync(Guid jobId, string errorMessage, CancellationToken ct = default);
}
