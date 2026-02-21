using System.IO.Compression;
using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Application.UseCases.Assets;

/// <summary>
/// 素材アップロードユースケース
/// 単体ファイルまたはZIPを受け取り、素材ライブラリに登録する
/// </summary>
public class UploadAssetUseCase
{
    private const long MaxFileSizeBytes = 50L * 1024 * 1024; // 50MB

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/webp", "image/gif",
    };

    private readonly IAssetRepository _assetRepository;
    private readonly IStorageService _storageService;

    public UploadAssetUseCase(IAssetRepository assetRepository, IStorageService storageService)
    {
        _assetRepository = assetRepository;
        _storageService = storageService;
    }

    /// <summary>
    /// 単体素材ファイルをアップロードする
    /// </summary>
    /// <returns>登録されたAssetのId</returns>
    public async Task<Guid> ExecuteSingleAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string category,
        CancellationToken ct = default)
    {
        if (!AllowedContentTypes.Contains(contentType))
            throw new InvalidOperationException($"サポートされていないファイル形式です: {contentType}");

        if (fileStream.CanSeek && fileStream.Length > MaxFileSizeBytes)
            throw new InvalidOperationException("ファイルサイズが上限（50MB）を超えています。");

        var fileSizeBytes = fileStream.CanSeek ? fileStream.Length : 0L;
        var storagePath = await _storageService.SaveAsync(fileStream, fileName, $"assets/{category}", ct);

        var asset = Asset.Create(
            name: Path.GetFileNameWithoutExtension(fileName),
            storagePath: storagePath,
            fileName: fileName,
            contentType: contentType,
            fileSizeBytes: fileSizeBytes,
            category: category);

        await _assetRepository.AddAsync(asset, ct);
        return asset.Id;
    }

    /// <summary>
    /// ZIPファイルを展開して素材を一括アップロードする
    /// </summary>
    /// <returns>登録されたAssetのIdリスト</returns>
    public async Task<IReadOnlyList<Guid>> ExecuteZipBatchAsync(
        Stream zipStream,
        string category,
        CancellationToken ct = default)
    {
        var assets = new List<Asset>();

        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);

        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Name)) continue; // ディレクトリエントリをスキップ
            if (entry.Length > MaxFileSizeBytes) continue;       // サイズ超過はスキップ

            var contentType = GetContentTypeFromExtension(Path.GetExtension(entry.Name));
            if (contentType is null) continue; // 非対応形式はスキップ

            await using var entryStream = entry.Open();
            var ms = new MemoryStream();
            await entryStream.CopyToAsync(ms, ct);
            ms.Position = 0;

            var storagePath = await _storageService.SaveAsync(ms, entry.Name, $"assets/{category}", ct);

            var asset = Asset.Create(
                name: Path.GetFileNameWithoutExtension(entry.Name),
                storagePath: storagePath,
                fileName: entry.Name,
                contentType: contentType,
                fileSizeBytes: entry.Length,
                category: category);

            assets.Add(asset);
        }

        await _assetRepository.AddRangeAsync(assets, ct);
        return assets.Select(a => a.Id).ToList();
    }

    private static string? GetContentTypeFromExtension(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => null,
        };
}
