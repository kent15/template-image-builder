namespace ImageBatchGenerator.Domain.Entities;

/// <summary>
/// 素材ライブラリ（商品画像・背景画像）
/// </summary>
public class Asset
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    /// <summary>素材ファイルのストレージパス</summary>
    public string StoragePath { get; private set; } = string.Empty;

    /// <summary>ファイル名</summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>MIMEタイプ（image/png, image/jpeg など）</summary>
    public string ContentType { get; private set; } = string.Empty;

    /// <summary>ファイルサイズ（バイト）</summary>
    public long FileSizeBytes { get; private set; }

    /// <summary>素材カテゴリ（ProductImage / Background）</summary>
    public string Category { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    private Asset() { }

    /// <summary>新しい素材を作成する</summary>
    public static Asset Create(
        string name,
        string storagePath,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string category,
        string? description = null)
    {
        return new Asset
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            StoragePath = storagePath,
            FileName = fileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            Category = category,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>素材を論理削除する</summary>
    public void SoftDelete()
    {
        IsActive = false;
        DeletedAt = DateTimeOffset.UtcNow;
    }
}
