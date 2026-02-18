namespace ImageBatchGenerator.Domain.Interfaces;

/// <summary>
/// ファイルストレージサービスインターフェース（Infrastructure層のLocalStorageで実装）
/// </summary>
public interface IStorageService
{
    // TODO: メソッド実装（Phase 2〜3）

    /// <summary>ファイルを保存し、ストレージパスを返す</summary>
    Task<string> SaveAsync(
        Stream content,
        string fileName,
        string subDirectory,
        CancellationToken ct = default);

    /// <summary>ストレージパスからファイルを読み込む</summary>
    Task<Stream> ReadAsync(string storagePath, CancellationToken ct = default);

    /// <summary>ストレージパスのファイルを削除する</summary>
    Task DeleteAsync(string storagePath, CancellationToken ct = default);

    /// <summary>ストレージパスが存在するか確認する</summary>
    Task<bool> ExistsAsync(string storagePath, CancellationToken ct = default);

    /// <summary>
    /// ジョブの生成画像群をZIPアーカイブとして取得する
    /// </summary>
    Task<Stream> CreateZipArchiveAsync(Guid jobId, CancellationToken ct = default);
}
