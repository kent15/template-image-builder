using System.Text.Json;
using ImageBatchGenerator.Application.DTOs;
using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Application.UseCases.Jobs;

/// <summary>
/// ジョブ作成ユースケース
/// InputRows が指定された場合はその入力データで JobItem を作成する
/// 未指定の場合は TotalCount 分の空の JobItem を生成する（Phase 1 互換）
/// </summary>
public class CreateJobUseCase
{
    private static readonly JsonSerializerOptions JsonOpts = new();

    private readonly IJobRepository _jobRepository;
    private readonly IJobItemRepository _jobItemRepository;

    public CreateJobUseCase(
        IJobRepository jobRepository,
        IJobItemRepository jobItemRepository)
    {
        _jobRepository = jobRepository;
        _jobItemRepository = jobItemRepository;
    }

    public async Task<JobDto> ExecuteAsync(CreateJobRequest request, CancellationToken ct = default)
    {
        var hasInputRows = request.InputRows is { Count: > 0 };
        var totalCount = hasInputRows ? request.InputRows!.Count : request.TotalCount;

        var job = Job.Create(
            name: request.Name,
            templateId: request.TemplateId,
            mappingRulesJson: request.MappingRulesJson,
            outputSettingsJson: request.OutputSettingsJson,
            totalCount: totalCount,
            csvOriginalFileName: request.CsvOriginalFileName,
            csvStoragePath: request.CsvStoragePath,
            createdBy: request.CreatedBy);

        await _jobRepository.AddAsync(job, ct);

        if (hasInputRows)
        {
            var items = request.InputRows!
                .Select((row, i) => JobItem.Create(
                    job.Id,
                    i,
                    JsonSerializer.Serialize(row, JsonOpts)))
                .ToList();

            await _jobItemRepository.AddRangeAsync(items, ct);
        }
        else if (totalCount > 0)
        {
            var items = Enumerable
                .Range(0, totalCount)
                .Select(i => JobItem.Create(job.Id, i))
                .ToList();

            await _jobItemRepository.AddRangeAsync(items, ct);
        }

        return job.ToDto();
    }
}
