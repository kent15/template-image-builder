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
        => await _channel.Writer.WriteAsync(jobId, ct);

    public async Task<Guid?> DequeueAsync(CancellationToken ct = default)
    {
        try
        {
            return await _channel.Reader.ReadAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    public Task<int> GetQueueLengthAsync()
        => Task.FromResult(_channel.Reader.Count);
}
