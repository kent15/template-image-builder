using ImageBatchGenerator.Application.DTOs;
using ImageBatchGenerator.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ImageBatchGenerator.Infrastructure.Notifications;

/// <summary>
/// SignalRを使用したリアルタイム進捗通知実装
/// ProgressHubを通じてブラウザにプッシュ配信する
/// </summary>
public class SignalRProgressNotifier : IProgressNotifier
{
    // TODO: Phase 3でIHubContext<ProgressHub>を注入
    // ProgressHubはWeb層に存在するため、循環参照を避けるためIHubContext<IProgressHubClient>等で注入する
    private readonly IHubContext<ProgressHubPlaceholder> _hubContext;

    public SignalRProgressNotifier(IHubContext<ProgressHubPlaceholder> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyProgressAsync(JobProgressDto progress, CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        // await _hubContext.Clients
        //     .Group(progress.JobId.ToString())
        //     .SendAsync("ProgressUpdated", progress, ct);
        throw new NotImplementedException();
    }

    public async Task NotifyCompletedAsync(Guid jobId, CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        // await _hubContext.Clients
        //     .Group(jobId.ToString())
        //     .SendAsync("JobCompleted", jobId, ct);
        throw new NotImplementedException();
    }

    public async Task NotifyErrorAsync(Guid jobId, string errorMessage, CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        // await _hubContext.Clients
        //     .Group(jobId.ToString())
        //     .SendAsync("JobError", new { jobId, errorMessage }, ct);
        throw new NotImplementedException();
    }
}

/// <summary>Phase 3 で実際の ProgressHub に差し替える仮クラス</summary>
public class ProgressHubPlaceholder : Hub { }
