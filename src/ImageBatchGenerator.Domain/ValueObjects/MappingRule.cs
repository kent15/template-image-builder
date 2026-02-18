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

    // TODO: バリデーションロジック実装（Phase 2）

    /// <summary>マッピングルールが有効かどうかを検証する</summary>
    public bool IsValid()
    {
        // TODO: LayerTypeに応じてCsvColumnNameまたはAssetIdが設定されているか検証する
        throw new NotImplementedException();
    }
}
