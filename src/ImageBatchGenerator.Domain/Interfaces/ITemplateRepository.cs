using ImageBatchGenerator.Domain.Entities;

namespace ImageBatchGenerator.Domain.Interfaces;

/// <summary>
/// Templateリポジトリインターフェース（Infrastructure層で実装）
/// </summary>
public interface ITemplateRepository
{
    // TODO: メソッド実装（Phase 2）

    Task<Template?> GetByIdAsync(Guid templateId, CancellationToken ct = default);

    Task<IReadOnlyList<Template>> GetAllActiveAsync(CancellationToken ct = default);

    Task AddAsync(Template template, CancellationToken ct = default);

    Task UpdateAsync(Template template, CancellationToken ct = default);

    /// <summary>論理削除する（DeletedAtを設定）</summary>
    Task SoftDeleteAsync(Guid templateId, CancellationToken ct = default);
}
