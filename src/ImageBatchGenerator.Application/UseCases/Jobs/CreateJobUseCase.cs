using ImageBatchGenerator.Application.DTOs;
using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Application.UseCases.Jobs;

/// <summary>
/// ジョブ作成ユースケース
/// CSVパース → JobItem一括生成 → DB保存
/// </summary>
public class CreateJobUseCase
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobItemRepository _jobItemRepository;
    private readonly ITemplateRepository _templateRepository;
    private readonly IStorageService _storageService;

    public CreateJobUseCase(
        IJobRepository jobRepository,
        IJobItemRepository jobItemRepository,
        ITemplateRepository templateRepository,
        IStorageService storageService)
    {
        _jobRepository = jobRepository;
        _jobItemRepository = jobItemRepository;
        _templateRepository = templateRepository;
        _storageService = storageService;
    }

    /// <summary>
    /// ジョブを作成しJobItemを一括生成する
    /// </summary>
    /// <returns>作成されたジョブのDTO</returns>
    public async Task<JobDto> ExecuteAsync(CreateJobRequest request, CancellationToken ct = default)
    {
        // TODO: Phase 2で実装
        // 1. テンプレート存在確認
        // 2. CSV読み込み・パース
        // 3. JobエンティティをCreated状態で作成
        // 4. JobItemを行ごとに生成（InputDataJsonにスナップショット保存）
        // 5. DBに保存
        // 6. JobDtoに変換して返却
        throw new NotImplementedException();
    }
}
