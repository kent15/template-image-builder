using System.Text.Json;
using System.Text.Json.Serialization;
using ImageBatchGenerator.Domain.ValueObjects;

namespace ImageBatchGenerator.Application.DTOs;

/// <summary>
/// ジョブの出力設定（Job.OutputSettingsJson のデシリアライズモデル）
/// </summary>
public record OutputSettings
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    [JsonPropertyName("format")]
    public string Format { get; init; } = "png";

    [JsonPropertyName("quality")]
    public int Quality { get; init; } = 90;

    public OutputFormat GetOutputFormat() =>
        Format.ToLowerInvariant() switch
        {
            "jpeg" or "jpg" => OutputFormat.Jpeg,
            "webp"          => OutputFormat.WebP,
            _               => OutputFormat.Png,
        };

    public static OutputSettings Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new OutputSettings();

        return JsonSerializer.Deserialize<OutputSettings>(json, JsonOpts)
            ?? new OutputSettings();
    }
}
