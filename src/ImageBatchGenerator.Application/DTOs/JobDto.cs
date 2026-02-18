namespace ImageBatchGenerator.Application.DTOs;

/// <summary>
/// ジョブ参照用DTO
/// </summary>
public record JobDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid TemplateId { get; init; }
    public string TemplateName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int TotalCount { get; init; }
    public int ProcessedCount { get; init; }
    public int SuccessCount { get; init; }
    public int WarningCount { get; init; }
    public int ErrorCount { get; init; }
    public int SkippedCount { get; init; }
    public string? CsvOriginalFileName { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? QueuedAt { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}
