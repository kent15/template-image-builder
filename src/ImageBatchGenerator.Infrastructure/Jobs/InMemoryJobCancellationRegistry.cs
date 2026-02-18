using System.Collections.Concurrent;
using ImageBatchGenerator.Application.Interfaces;

namespace ImageBatchGenerator.Infrastructure.Jobs;

/// <summary>
/// インメモリ実装の IJobCancellationRegistry
/// Singleton で登録すること。
/// </summary>
public class InMemoryJobCancellationRegistry : IJobCancellationRegistry
{
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _registry = new();

    public CancellationToken Register(Guid jobId)
    {
        var cts = new CancellationTokenSource();
        _registry[jobId] = cts;
        return cts.Token;
    }

    public void Cancel(Guid jobId)
    {
        if (_registry.TryGetValue(jobId, out var cts))
            cts.Cancel();
    }

    public void Unregister(Guid jobId)
    {
        if (_registry.TryRemove(jobId, out var cts))
            cts.Dispose();
    }
}
