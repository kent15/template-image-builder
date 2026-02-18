using System.Collections.Concurrent;
using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Infrastructure.Persistence.Repositories;

/// <summary>
/// Template リポジトリのインメモリ実装（Phase 1）
/// Phase 2 以降は EF Core 実装（TemplateRepository）に差し替える
/// </summary>
public class InMemoryTemplateRepository : ITemplateRepository
{
    private readonly ConcurrentDictionary<Guid, Template> _store = new();

    public Task<Template?> GetByIdAsync(Guid templateId, CancellationToken ct = default)
        => Task.FromResult(_store.TryGetValue(templateId, out var t) ? t : null);

    public Task<IReadOnlyList<Template>> GetAllActiveAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Template> result = _store.Values.ToList();
        return Task.FromResult(result);
    }

    public Task AddAsync(Template template, CancellationToken ct = default)
    {
        _store[template.Id] = template;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Template template, CancellationToken ct = default)
    {
        _store[template.Id] = template;
        return Task.CompletedTask;
    }

    public Task SoftDeleteAsync(Guid templateId, CancellationToken ct = default)
    {
        _store.TryRemove(templateId, out _);
        return Task.CompletedTask;
    }
}
