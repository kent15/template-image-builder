using Microsoft.AspNetCore.SignalR;

namespace ImageBatchGenerator.Web.Hubs;

/// <summary>
/// リアルタイム進捗通知SignalRハブ
/// クライアントはジョブIDをグループとして購読する
/// 切断時はクライアント側でポーリング（5秒間隔）にフォールバックする
/// </summary>
public class ProgressHub : Hub
{
    // TODO: Phase 3で実装

    /// <summary>
    /// クライアントが指定ジョブの進捗通知グループに参加する
    /// </summary>
    public async Task JoinJobGroup(string jobId)
    {
        // TODO: Phase 3で実装
        // await Groups.AddToGroupAsync(Context.ConnectionId, jobId);
        throw new NotImplementedException();
    }

    /// <summary>
    /// クライアントが指定ジョブの進捗通知グループから離脱する
    /// </summary>
    public async Task LeaveJobGroup(string jobId)
    {
        // TODO: Phase 3で実装
        // await Groups.RemoveFromGroupAsync(Context.ConnectionId, jobId);
        throw new NotImplementedException();
    }

    public override async Task OnConnectedAsync()
    {
        // TODO: Phase 3で接続ログを記録する
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // TODO: Phase 3で切断ログを記録する
        await base.OnDisconnectedAsync(exception);
    }
}
