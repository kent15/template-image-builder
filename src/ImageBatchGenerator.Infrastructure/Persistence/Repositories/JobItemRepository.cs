using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.Interfaces;
using ImageBatchGenerator.Domain.ValueObjects;
using ImageBatchGenerator.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ImageBatchGenerator.Infrastructure.Persistence.Repositories;

/// <summary>
/// JobItemリポジトリ実装（EF Core + SQL Server）
/// </summary>
public class JobItemRepository : IJobItemRepository
{
    private readonly AppDbContext _context;

    public JobItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<JobItem?> GetByIdAsync(Guid jobItemId, CancellationToken ct = default)
    {
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<JobItem>> GetByJobIdAsync(Guid jobId, CancellationToken ct = default)
    {
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<JobItem>> GetByJobIdAndStatusAsync(
        Guid jobId,
        JobItemStatus status,
        CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<JobItem>> GetPendingAfterIndexAsync(
        Guid jobId,
        int fromIndex,
        CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        // RowIndex >= fromIndex AND Status = Pending の件をRowIndex昇順で取得
        throw new NotImplementedException();
    }

    public async Task AddRangeAsync(IEnumerable<JobItem> items, CancellationToken ct = default)
    {
        // TODO: Phase 2で実装
        // 大量件数のバルクインサートを考慮する
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(JobItem item, CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        throw new NotImplementedException();
    }

    public async Task UpdateRangeAsync(IEnumerable<JobItem> items, CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        throw new NotImplementedException();
    }
}
