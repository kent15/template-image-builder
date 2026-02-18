using System.Collections.Concurrent;
using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.Interfaces;
using ImageBatchGenerator.Domain.ValueObjects;

namespace ImageBatchGenerator.Infrastructure.Persistence.Repositories;

/// <summary>
/// インメモリ実装の IJobItemRepository
/// DB 未接続フェーズ（Phase 1）で使用。Singleton で登録すること。
/// </summary>
public class InMemoryJobItemRepository : IJobItemRepository
{
    private readonly ConcurrentDictionary<Guid, JobItem> _store = new();

    public Task<JobItem?> GetByIdAsync(Guid jobItemId, CancellationToken ct = default)
        => Task.FromResult(_store.TryGetValue(jobItemId, out var item) ? item : null);

    public Task<IReadOnlyList<JobItem>> GetByJobIdAsync(Guid jobId, CancellationToken ct = default)
    {
        var list = _store.Values
            .Where(i => i.JobId == jobId)
            .OrderBy(i => i.RowIndex)
            .ToList();
        return Task.FromResult<IReadOnlyList<JobItem>>(list);
    }

    public Task<IReadOnlyList<JobItem>> GetByJobIdAndStatusAsync(
        Guid jobId, JobItemStatus status, CancellationToken ct = default)
    {
        var list = _store.Values
            .Where(i => i.JobId == jobId && i.Status == status)
            .OrderBy(i => i.RowIndex)
            .ToList();
        return Task.FromResult<IReadOnlyList<JobItem>>(list);
    }

    public Task<IReadOnlyList<JobItem>> GetPendingAfterIndexAsync(
        Guid jobId, int fromIndex, CancellationToken ct = default)
    {
        var list = _store.Values
            .Where(i => i.JobId == jobId
                     && i.Status == JobItemStatus.Pending
                     && i.RowIndex >= fromIndex)
            .OrderBy(i => i.RowIndex)
            .ToList();
        return Task.FromResult<IReadOnlyList<JobItem>>(list);
    }

    public Task AddRangeAsync(IEnumerable<JobItem> items, CancellationToken ct = default)
    {
        foreach (var item in items)
            _store[item.Id] = item;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(JobItem item, CancellationToken ct = default)
    {
        _store[item.Id] = item;
        return Task.CompletedTask;
    }

    public Task UpdateRangeAsync(IEnumerable<JobItem> items, CancellationToken ct = default)
    {
        foreach (var item in items)
            _store[item.Id] = item;
        return Task.CompletedTask;
    }
}
