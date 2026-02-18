using System.Threading.Channels;
using ImageBatchGenerator.Application.Interfaces;

namespace ImageBatchGenerator.Infrastructure.Queue;

/// <summary>
/// インメモリジョブキュー実装（System.Threading.Channels）
/// MVP構成用。将来はRabbitMqJobQueueに差し替え可能。
/// </summary>
public class InMemoryJobQueue : IJobQueue
{
    // TODO: Phase 3で容量はappsettingsから注入する
    private readonly Channel<Guid> _channel = Channel.CreateBounded<Guid>(capacity: 100);

    public async Task EnqueueAsync(Guid jobId, CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        // await _channel.Writer.WriteAsync(jobId, ct);
        throw new NotImplementedException();
    }

    public async Task<Guid?> DequeueAsync(CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        // try { return await _channel.Reader.ReadAsync(ct); }
        // catch (OperationCanceledException) { return null; }
        throw new NotImplementedException();
    }

    public async Task<int> GetQueueLengthAsync()
    {
        // TODO: Phase 3で実装
        // return _channel.Reader.Count;
        throw new NotImplementedException();
    }
}
