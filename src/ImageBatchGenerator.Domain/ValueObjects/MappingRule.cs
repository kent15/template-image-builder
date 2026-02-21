namespace ImageBatchGenerator.Domain.ValueObjects;

/// <summary>
/// テンプレートレイヤーとCSV列のマッピングルール値オブジェクト
/// </summary>
public record MappingRule
{
    /// <summary>テンプレートのレイヤーID</summary>
    public string LayerId { get; init; } = string.Empty;

    /// <summary>レイヤー種別（Text / ProductImage / Background）</summary>
    public string LayerType { get; init; } = string.Empty;

    /// <summary>マッピング元のCSV列名（テキストレイヤーの場合）</summary>
    public string? CsvColumnName { get; init; }

    /// <summary>固定素材ID（背景・固定画像の場合）</summary>
    public Guid? AssetId { get; init; }

    /// <summary>マッピングルールが有効かどうかを検証する</summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(LayerId)) return false;

        return LayerType switch
        {
            "Text" => !string.IsNullOrWhiteSpace(CsvColumnName),
            "ProductImage" => !string.IsNullOrWhiteSpace(CsvColumnName) || AssetId.HasValue,
            "Background" => AssetId.HasValue,
            _ => false,
        };
    }
}
