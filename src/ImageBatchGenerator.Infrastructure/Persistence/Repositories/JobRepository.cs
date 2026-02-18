using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.Interfaces;
using ImageBatchGenerator.Domain.ValueObjects;
using ImageBatchGenerator.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ImageBatchGenerator.Infrastructure.Persistence.Repositories;

/// <summary>
/// Jobリポジトリ実装（EF Core + SQL Server）
/// </summary>
public class JobRepository : IJobRepository
{
    private readonly AppDbContext _context;

    public JobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Job?> GetByIdAsync(Guid jobId, CancellationToken ct = default)
    {
        // TODO: Phase 1で実装
        // return await _context.Jobs
        //     .Include(j => j.Template)
        //     .FirstOrDefaultAsync(j => j.Id == jobId, ct);
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken ct = default)
    {
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<Job>> GetByStatusAsync(JobStatus status, CancellationToken ct = default)
    {
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }

    public async Task<Job?> DequeueNextAsync(CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        // Queued状態のジョブを作成日時順（昇順）で1件取得する
        throw new NotImplementedException();
    }

    public async Task AddAsync(Job job, CancellationToken ct = default)
    {
        // TODO: Phase 1で実装
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(Job job, CancellationToken ct = default)
    {
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }

    public async Task UpdateCheckpointAsync(Guid jobId, int lastProcessedIndex, CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        // チェックポイント更新は頻度が高いため、ExecuteUpdateAsync等で最小限のカラムのみ更新する
        throw new NotImplementedException();
    }
}
