using ImageBatchGenerator.Domain.Entities;

namespace ImageBatchGenerator.Application.DTOs;

/// <summary>
/// Job エンティティ → JobDto 変換
/// </summary>
public static class JobDtoMapper
{
    public static JobDto ToDto(this Job job) => new()
    {
        Id = job.Id,
        Name = job.Name,
        TemplateId = job.TemplateId,
        TemplateName = string.Empty, // Phase 2: ITemplateRepository から名前を引く
        Status = job.Status.ToString(),
        TotalCount = job.TotalCount,
        ProcessedCount = job.ProcessedCount,
        SuccessCount = job.SuccessCount,
        WarningCount = job.WarningCount,
        ErrorCount = job.ErrorCount,
        SkippedCount = job.SkippedCount,
        CsvOriginalFileName = job.CsvOriginalFileName,
        CreatedAt = job.CreatedAt,
        QueuedAt = job.QueuedAt,
        StartedAt = job.StartedAt,
        CompletedAt = job.CompletedAt,
    };
}
