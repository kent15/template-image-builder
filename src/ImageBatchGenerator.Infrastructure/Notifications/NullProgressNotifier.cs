using ImageBatchGenerator.Application.DTOs;
using ImageBatchGenerator.Application.Interfaces;

namespace ImageBatchGenerator.Infrastructure.Notifications;

/// <summary>
/// 進捗通知の Null 実装
/// SignalR が未接続の間はこの実装を使用する（Phase 3 で SignalRProgressNotifier に差し替え）
/// </summary>
public class NullProgressNotifier : IProgressNotifier
{
    public Task NotifyProgressAsync(JobProgressDto progress, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task NotifyCompletedAsync(Guid jobId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task NotifyErrorAsync(Guid jobId, string errorMessage, CancellationToken ct = default)
        => Task.CompletedTask;
}
