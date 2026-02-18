using System.Collections.Concurrent;
using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.Interfaces;
using ImageBatchGenerator.Domain.ValueObjects;

namespace ImageBatchGenerator.Infrastructure.Persistence.Repositories;

/// <summary>
/// インメモリ実装の IJobRepository
/// DB 未接続フェーズ（Phase 1）で使用。Singleton で登録すること。
/// </summary>
public class InMemoryJobRepository : IJobRepository
{
    private readonly ConcurrentDictionary<Guid, Job> _store = new();

    public Task<Job?> GetByIdAsync(Guid jobId, CancellationToken ct = default)
        => Task.FromResult(_store.TryGetValue(jobId, out var job) ? job : null);

    public Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken ct = default)
    {
        var list = _store.Values
            .OrderByDescending(j => j.CreatedAt)
            .ToList();
        return Task.FromResult<IReadOnlyList<Job>>(list);
    }

    public Task<IReadOnlyList<Job>> GetByStatusAsync(JobStatus status, CancellationToken ct = default)
    {
        var list = _store.Values
            .Where(j => j.Status == status)
            .OrderByDescending(j => j.CreatedAt)
            .ToList();
        return Task.FromResult<IReadOnlyList<Job>>(list);
    }

    public Task<Job?> DequeueNextAsync(CancellationToken ct = default)
    {
        var job = _store.Values
            .Where(j => j.Status == JobStatus.Queued)
            .OrderBy(j => j.QueuedAt)
            .FirstOrDefault();
        return Task.FromResult(job);
    }

    public Task AddAsync(Job job, CancellationToken ct = default)
    {
        _store[job.Id] = job;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Job job, CancellationToken ct = default)
    {
        _store[job.Id] = job;
        return Task.CompletedTask;
    }

    public Task UpdateCheckpointAsync(Guid jobId, int lastProcessedIndex, CancellationToken ct = default)
    {
        if (_store.TryGetValue(jobId, out var job))
            job.UpdateCheckpoint(lastProcessedIndex);
        return Task.CompletedTask;
    }
}
