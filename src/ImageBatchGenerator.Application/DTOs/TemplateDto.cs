using ImageBatchGenerator.Domain.Entities;

namespace ImageBatchGenerator.Application.DTOs;

/// <summary>
/// テンプレート情報 DTO
/// </summary>
public record TemplateDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public string LayerConfigJson { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public static class TemplateDtoMapper
{
    public static TemplateDto ToDto(this Template template) => new()
    {
        Id = template.Id,
        Name = template.Name,
        Description = template.Description,
        Width = template.Width,
        Height = template.Height,
        LayerConfigJson = template.LayerConfigJson,
        IsActive = template.IsActive,
        CreatedAt = template.CreatedAt,
    };
}
