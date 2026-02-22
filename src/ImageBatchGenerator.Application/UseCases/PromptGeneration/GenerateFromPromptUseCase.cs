using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Application.UseCases.PromptGeneration;

/// <summary>
/// テキストプロンプトから画像を1枚生成するユースケース。
/// 参照素材IDが指定された場合はその素材パスを生成エンジンに渡す。
/// </summary>
public class GenerateFromPromptUseCase
{
    private readonly IPromptImageGenerator _generator;
    private readonly IAssetRepository _assetRepository;
    private readonly IStorageService _storageService;

    public GenerateFromPromptUseCase(
        IPromptImageGenerator generator,
        IAssetRepository assetRepository,
        IStorageService storageService)
    {
        _generator = generator;
        _assetRepository = assetRepository;
        _storageService = storageService;
    }

    public async Task<GenerateFromPromptResult> ExecuteAsync(
        GenerateFromPromptRequest request,
        CancellationToken ct = default)
    {
        // 参照素材のストレージパスを解決
        string? assetStoragePath = null;
        if (request.ReferenceAssetId.HasValue)
        {
            var asset = await _assetRepository.GetByIdAsync(request.ReferenceAssetId.Value, ct);
            assetStoragePath = asset?.StoragePath;
        }

        // 画像生成
        var imageBytes = await _generator.GenerateAsync(
            request.Prompt,
            assetStoragePath,
            request.Width,
            request.Height,
            ct);

        var ext = request.Format.ToLowerInvariant() switch
        {
            "jpeg" or "jpg" => "jpg",
            "webp" => "webp",
            _ => "png",
        };

        var mimeType = ext switch
        {
            "jpg" => "image/jpeg",
            "webp" => "image/webp",
            _ => "image/png",
        };

        var fileName = $"prompt_{DateTime.UtcNow:yyyyMMddHHmmssff}.{ext}";

        // ストレージに保存（履歴として残す）
        using var ms = new MemoryStream(imageBytes);
        await _storageService.SaveAsync(ms, fileName, "prompt-output", ct);

        return new GenerateFromPromptResult
        {
            FileName = fileName,
            MimeType = mimeType,
            ImageData = imageBytes,
        };
    }
}
