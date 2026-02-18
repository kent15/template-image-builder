namespace ImageBatchGenerator.Application.DTOs;

/// <summary>
/// ジョブ作成リクエストDTO（ウィザード4ステップの確定データ）
/// </summary>
public record CreateJobRequest
{
    public string Name { get; init; } = string.Empty;
    public Guid TemplateId { get; init; }

    /// <summary>テキスト・画像レイヤーとCSV列のマッピングルール（JSON直列化形式）</summary>
    public string MappingRulesJson { get; init; } = string.Empty;

    /// <summary>出力設定（フォーマット・サイズ・品質・命名規則等のJSON直列化形式）</summary>
    public string OutputSettingsJson { get; init; } = string.Empty;

    public string? CsvOriginalFileName { get; init; }
    public string? CsvStoragePath { get; init; }
    public string? CreatedBy { get; init; }

    // TODO: バリデーションロジック実装（Phase 2）
}
