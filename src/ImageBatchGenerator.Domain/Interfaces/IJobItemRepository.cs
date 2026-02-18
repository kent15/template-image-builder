using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.ValueObjects;

namespace ImageBatchGenerator.Domain.Interfaces;

/// <summary>
/// JobItemリポジトリインターフェース（Infrastructure層で実装）
/// </summary>
public interface IJobItemRepository
{
    // TODO: メソッド実装（Phase 2〜3）

    Task<JobItem?> GetByIdAsync(Guid jobItemId, CancellationToken ct = default);

    Task<IReadOnlyList<JobItem>> GetByJobIdAsync(Guid jobId, CancellationToken ct = default);

    /// <summary>指定ステータスのジョブ明細一覧を取得する</summary>
    Task<IReadOnlyList<JobItem>> GetByJobIdAndStatusAsync(
        Guid jobId,
        JobItemStatus status,
        CancellationToken ct = default);

    /// <summary>チェックポイント以降のPending件を取得する（再開用）</summary>
    Task<IReadOnlyList<JobItem>> GetPendingAfterIndexAsync(
        Guid jobId,
        int fromIndex,
        CancellationToken ct = default);

    Task AddRangeAsync(IEnumerable<JobItem> items, CancellationToken ct = default);

    Task UpdateAsync(JobItem item, CancellationToken ct = default);

    Task UpdateRangeAsync(IEnumerable<JobItem> items, CancellationToken ct = default);
}
