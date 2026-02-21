using System.Collections.Concurrent;
using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Infrastructure.Persistence.Repositories;

/// <summary>
/// インメモリ実装の IAssetRepository
/// DB 未接続フェーズ（Phase 1-2）で使用。Singleton で登録すること。
/// </summary>
public class InMemoryAssetRepository : IAssetRepository
{
    private readonly ConcurrentDictionary<Guid, Asset> _store = new();

    public Task<Asset?> GetByIdAsync(Guid assetId, CancellationToken ct = default)
        => Task.FromResult(_store.TryGetValue(assetId, out var asset) ? asset : null);

    public Task<IReadOnlyList<Asset>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var list = _store.Values
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .ToList();
        return Task.FromResult<IReadOnlyList<Asset>>(list);
    }

    public Task<IReadOnlyList<Asset>> GetByCategoryAsync(string category, CancellationToken ct = default)
    {
        var list = _store.Values
            .Where(a => a.IsActive && string.Equals(a.Category, category, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(a => a.CreatedAt)
            .ToList();
        return Task.FromResult<IReadOnlyList<Asset>>(list);
    }

    public Task AddAsync(Asset asset, CancellationToken ct = default)
    {
        _store[asset.Id] = asset;
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<Asset> assets, CancellationToken ct = default)
    {
        foreach (var asset in assets)
            _store[asset.Id] = asset;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Asset asset, CancellationToken ct = default)
    {
        _store[asset.Id] = asset;
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(Guid assetId, CancellationToken ct = default)
    {
        if (_store.TryGetValue(assetId, out var asset))
            asset.SoftDelete();
        return Task.CompletedTask;
    }
}
