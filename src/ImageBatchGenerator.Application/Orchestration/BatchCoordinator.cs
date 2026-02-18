using ImageBatchGenerator.Application.Interfaces;
using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Application.Orchestration;

/// <summary>
/// バッチ並列処理コーディネーター
/// System.Threading.Channelsを使用したProducer-Consumerパターンで並列実行する
/// </summary>
public class BatchCoordinator
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobItemRepository _jobItemRepository;
    private readonly IImageProcessor _imageProcessor;
    private readonly IStorageService _storageService;
    private readonly IProgressNotifier _progressNotifier;

    // TODO: appsettingsから注入（Phase 3）
    private readonly int _maxDegreeOfParallelism = 2;

    public BatchCoordinator(
        IJobRepository jobRepository,
        IJobItemRepository jobItemRepository,
        IImageProcessor imageProcessor,
        IStorageService storageService,
        IProgressNotifier progressNotifier)
    {
        _jobRepository = jobRepository;
        _jobItemRepository = jobItemRepository;
        _imageProcessor = imageProcessor;
        _storageService = storageService;
        _progressNotifier = progressNotifier;
    }

    /// <summary>
    /// JobItemリストを並列処理する
    /// </summary>
    /// <param name="job">処理対象ジョブ</param>
    /// <param name="items">処理対象明細リスト</param>
    /// <param name="ct">キャンセルトークン</param>
    public async Task ProcessAsync(
        Job job,
        IReadOnlyList<JobItem> items,
        CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        // var channel = Channel.CreateBounded<JobItem>(_maxDegreeOfParallelism * 2);
        //
        // // Producer: JobItemをChannelに書き込む
        // var producer = ProduceJobItemsAsync(items, channel.Writer, ct);
        //
        // // Consumer: 並列ワーカーがChannelを購読して画像生成する
        // var consumers = Enumerable
        //     .Range(0, _maxDegreeOfParallelism)
        //     .Select(_ => ConsumeAsync(job, channel.Reader, ct))
        //     .ToArray();
        //
        // await Task.WhenAll(producer);
        // channel.Writer.Complete();
        // await Task.WhenAll(consumers);
        throw new NotImplementedException();
    }

    private async Task ProduceJobItemsAsync(
        IReadOnlyList<JobItem> items,
        object writer,
        CancellationToken ct)
    {
        // TODO: Phase 3で実装（ChannelWriter<JobItem>を使用）
        throw new NotImplementedException();
    }

    private async Task ConsumeAsync(Job job, object reader, CancellationToken ct)
    {
        // TODO: Phase 3で実装（ChannelReader<JobItem>を使用）
        // 1. Channelから1件取り出す
        // 2. IImageProcessor.GenerateAsync()で画像生成
        // 3. IStorageService.SaveAsync()で保存
        // 4. JobItem.MarkSuccess/Warning/Error()でステータス更新
        // 5. 100件ごとにLastProcessedIndexをDB更新（チェックポイント）
        // 6. IProgressNotifier.NotifyProgressAsync()で進捗通知
        throw new NotImplementedException();
    }
}
