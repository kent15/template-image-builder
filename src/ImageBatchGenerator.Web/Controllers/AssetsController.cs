using ImageBatchGenerator.Application.UseCases.Assets;
using ImageBatchGenerator.Domain.Interfaces;
using ImageBatchGenerator.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImageBatchGenerator.Web.Controllers;

/// <summary>
/// 素材ライブラリAPIコントローラー
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly IAssetRepository _assetRepository;
    private readonly UploadAssetUseCase _uploadAssetUseCase;

    public AssetsController(IAssetRepository assetRepository, UploadAssetUseCase uploadAssetUseCase)
    {
        _assetRepository = assetRepository;
        _uploadAssetUseCase = uploadAssetUseCase;
    }

    /// <summary>素材一覧を取得する（categoryを指定するとカテゴリ絞り込み）</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category, CancellationToken ct)
    {
        var assets = string.IsNullOrWhiteSpace(category)
            ? await _assetRepository.GetAllActiveAsync(ct)
            : await _assetRepository.GetByCategoryAsync(category, ct);

        var result = assets.Select(a => new
        {
            a.Id,
            a.Name,
            a.Description,
            a.FileName,
            a.ContentType,
            a.FileSizeBytes,
            a.Category,
            a.StoragePath,
            a.IsActive,
            a.CreatedAt,
        });

        return Ok(result);
    }

    /// <summary>
    /// 素材を単体アップロードする
    /// 対応形式: image/png, image/jpeg, image/webp, image/gif（最大50MB）
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] UploadAssetRequest request, CancellationToken ct)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(new { error = "ファイルが指定されていません。" });

        await using var stream = request.File.OpenReadStream();
        var assetId = await _uploadAssetUseCase.ExecuteSingleAsync(
            fileStream: stream,
            fileName: request.File.FileName,
            contentType: request.File.ContentType,
            category: request.Category,
            ct: ct);

        return Ok(new { id = assetId });
    }

    /// <summary>ZIPファイルから素材を一括アップロードする</summary>
    [HttpPost("batch")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadBatch([FromForm] UploadBatchAssetRequest request, CancellationToken ct)
    {
        if (request.ZipFile is null || request.ZipFile.Length == 0)
            return BadRequest(new { error = "ZIPファイルが指定されていません。" });

        await using var stream = request.ZipFile.OpenReadStream();
        var assetIds = await _uploadAssetUseCase.ExecuteZipBatchAsync(
            zipStream: stream,
            category: request.Category,
            ct: ct);

        return Ok(new { count = assetIds.Count, ids = assetIds });
    }

    /// <summary>素材を論理削除する</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var asset = await _assetRepository.GetByIdAsync(id, ct);
        if (asset is null) return NotFound();

        await _assetRepository.SoftDeleteAsync(id, ct);
        return NoContent();
    }
}
