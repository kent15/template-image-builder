namespace ImageBatchGenerator.Domain.Entities;

/// <summary>
/// 生成画像メタ情報
/// </summary>
public class GeneratedImage
{
    public Guid Id { get; private set; }
    public Guid JobItemId { get; private set; }
    public Guid JobId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }

    /// <summary>出力フォーマット（PNG / JPEG / WEBP）</summary>
    public string Format { get; private set; } = string.Empty;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool IsAvailable { get; private set; }

    /// <summary>TTL削除期限</summary>
    public DateTimeOffset? ExpiresAt { get; private set; }

    public DateTimeOffset GeneratedAt { get; private set; }

    public JobItem? JobItem { get; private set; }
    public Job? Job { get; private set; }

    // TODO: ドメインロジック実装（Phase 3）

    /// <summary>生成画像を利用不可状態にする（TTL期限切れ等）</summary>
    public void MarkUnavailable()
    {
        // TODO: IsAvailableをfalseにする
        throw new NotImplementedException();
    }
}
