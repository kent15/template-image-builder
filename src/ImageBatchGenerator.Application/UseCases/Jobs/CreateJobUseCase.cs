using ImageBatchGenerator.Application.DTOs;
using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Application.UseCases.Jobs;

/// <summary>
/// ジョブ作成ユースケース
/// インメモリ実装: CSVパースは行わず TotalCount 分の JobItem を生成する
/// </summary>
public class CreateJobUseCase
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobItemRepository _jobItemRepository;

    // Phase 2以降で使用: ITemplateRepository, IStorageService
    public CreateJobUseCase(
        IJobRepository jobRepository,
        IJobItemRepository jobItemRepository)
    {
        _jobRepository = jobRepository;
        _jobItemRepository = jobItemRepository;
    }

    /// <summary>
    /// ジョブを作成し TotalCount 分の JobItem を一括生成する
    /// </summary>
    public async Task<JobDto> ExecuteAsync(CreateJobRequest request, CancellationToken ct = default)
    {
        var job = Job.Create(
            name: request.Name,
            templateId: request.TemplateId,
            mappingRulesJson: request.MappingRulesJson,
            outputSettingsJson: request.OutputSettingsJson,
            totalCount: request.TotalCount,
            csvOriginalFileName: request.CsvOriginalFileName,
            csvStoragePath: request.CsvStoragePath,
            createdBy: request.CreatedBy);

        await _jobRepository.AddAsync(job, ct);

        if (request.TotalCount > 0)
        {
            var items = Enumerable
                .Range(0, request.TotalCount)
                .Select(i => JobItem.Create(job.Id, i))
                .ToList();

            await _jobItemRepository.AddRangeAsync(items, ct);
        }

        return job.ToDto();
    }
}
