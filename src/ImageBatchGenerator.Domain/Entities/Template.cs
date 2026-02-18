namespace ImageBatchGenerator.Domain.Entities;

/// <summary>
/// 画像テンプレートマスタ
/// </summary>
public class Template
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    /// <summary>テンプレートファイルのストレージパス（SVG/JSON）</summary>
    public string FilePath { get; private set; } = string.Empty;

    public string? ThumbnailPath { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    /// <summary>レイヤー構成定義JSON（テキスト・背景・商品画像の差し替え領域）</summary>
    public string LayerConfigJson { get; private set; } = string.Empty;

    public int Version { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public IReadOnlyCollection<Job> Jobs { get; private set; } = new List<Job>();

    // TODO: ドメインロジック実装（Phase 2）

    /// <summary>テンプレートを新しいバージョンに更新する</summary>
    public void UpdateVersion(string layerConfigJson, string filePath)
    {
        // TODO: Versionをインクリメントし、LayerConfigJson・FilePathを更新する
        throw new NotImplementedException();
    }

    /// <summary>テンプレートを論理削除する</summary>
    public void SoftDelete()
    {
        // TODO: DeletedAtを設定し、IsActiveをfalseにする
        throw new NotImplementedException();
    }

    /// <summary>サムネイルパスを設定する</summary>
    public void SetThumbnail(string thumbnailPath)
    {
        // TODO: ThumbnailPathを更新する
        throw new NotImplementedException();
    }
}
