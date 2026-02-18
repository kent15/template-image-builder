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

    /// <summary>
    /// CSV行数（インメモリ管理ではCSVパース不要のため呼び出し元から指定）
    /// Phase 2以降は CSVパース結果で自動設定する
    /// </summary>
    public int TotalCount { get; init; } = 0;
}
