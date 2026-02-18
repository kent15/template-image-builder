using ImageBatchGenerator.Domain.Entities;

namespace ImageBatchGenerator.Domain.Interfaces;

/// <summary>
/// Assetリポジトリインターフェース（Infrastructure層で実装）
/// </summary>
public interface IAssetRepository
{
    // TODO: メソッド実装（Phase 2）

    Task<Asset?> GetByIdAsync(Guid assetId, CancellationToken ct = default);

    Task<IReadOnlyList<Asset>> GetAllActiveAsync(CancellationToken ct = default);

    /// <summary>カテゴリ別に素材を取得する</summary>
    Task<IReadOnlyList<Asset>> GetByCategoryAsync(string category, CancellationToken ct = default);

    Task AddAsync(Asset asset, CancellationToken ct = default);

    Task AddRangeAsync(IEnumerable<Asset> assets, CancellationToken ct = default);

    Task UpdateAsync(Asset asset, CancellationToken ct = default);

    /// <summary>論理削除する</summary>
    Task SoftDeleteAsync(Guid assetId, CancellationToken ct = default);
}
