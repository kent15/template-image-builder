using ImageBatchGenerator.Application.UseCases.Assets;
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
    private readonly UploadAssetUseCase _uploadAssetUseCase;

    public AssetsController(UploadAssetUseCase uploadAssetUseCase)
    {
        _uploadAssetUseCase = uploadAssetUseCase;
    }

    /// <summary>素材一覧を取得する</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category, CancellationToken ct)
    {
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }

    /// <summary>素材を単体アップロードする</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] UploadAssetRequest request, CancellationToken ct)
    {
        // TODO: Phase 2で実装
        // MIMEタイプ・ファイルサイズバリデーション（最大50MB）
        throw new NotImplementedException();
    }

    /// <summary>ZIPファイルから素材を一括アップロードする</summary>
    [HttpPost("batch")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadBatch([FromForm] UploadBatchAssetRequest request, CancellationToken ct)
    {
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }

    /// <summary>素材を論理削除する</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }
}
