using ImageBatchGenerator.Application.Interfaces;
using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Application.Orchestration;

/// <summary>
/// ジョブオーケストレーター
/// JobQueueWorkerから呼び出され、1ジョブの処理フロー全体を統括する
/// </summary>
public class JobOrchestrator
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobItemRepository _jobItemRepository;
    private readonly ITemplateRepository _templateRepository;
    private readonly IImageProcessor _imageProcessor;
    private readonly IStorageService _storageService;
    private readonly IProgressNotifier _progressNotifier;
    private readonly BatchCoordinator _batchCoordinator;

    public JobOrchestrator(
        IJobRepository jobRepository,
        IJobItemRepository jobItemRepository,
        ITemplateRepository templateRepository,
        IImageProcessor imageProcessor,
        IStorageService storageService,
        IProgressNotifier progressNotifier,
        BatchCoordinator batchCoordinator)
    {
        _jobRepository = jobRepository;
        _jobItemRepository = jobItemRepository;
        _templateRepository = templateRepository;
        _imageProcessor = imageProcessor;
        _storageService = storageService;
        _progressNotifier = progressNotifier;
        _batchCoordinator = batchCoordinator;
    }

    /// <summary>
    /// 1ジョブの処理を開始から完了まで統括する
    /// </summary>
    public async Task ExecuteJobAsync(Guid jobId, CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        // 1. ジョブ・テンプレート取得
        // 2. Job.Start()でRunning状態に変更
        // 3. BatchCoordinatorに並列処理を委譲
        // 4. 完了/失敗に応じてJob.Complete()を呼び出す
        // 5. SignalRで完了通知
        throw new NotImplementedException();
    }

    /// <summary>
    /// チェックポイントから処理を再開する
    /// </summary>
    public async Task ResumeJobAsync(Guid jobId, CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        // 1. ジョブのLastProcessedIndex以降のPending件を取得
        // 2. BatchCoordinatorに処理を委譲
        throw new NotImplementedException();
    }
}
