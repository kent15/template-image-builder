using ImageBatchGenerator.Application.UseCases.PromptGeneration;
using Microsoft.AspNetCore.Mvc;

namespace ImageBatchGenerator.Web.Controllers;

/// <summary>
/// テキストプロンプトから画像を生成するAPIコントローラー。
/// 参照素材IDを指定するとその素材を背景として使用する。
/// </summary>
[ApiController]
[Route("api/generate")]
public class PromptGenerationController : ControllerBase
{
    private readonly GenerateFromPromptUseCase _useCase;

    public PromptGenerationController(GenerateFromPromptUseCase useCase)
    {
        _useCase = useCase;
    }

    /// <summary>
    /// プロンプトから画像を1枚生成してバイナリで返す。
    /// </summary>
    /// <param name="request">プロンプト・参照素材ID・サイズ・フォーマット</param>
    /// <param name="ct">キャンセルトークン</param>
    [HttpPost]
    public async Task<IActionResult> Generate(
        [FromBody] GeneratePromptApiRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest(new { error = "プロンプトを入力してください。" });

        var result = await _useCase.ExecuteAsync(new GenerateFromPromptRequest
        {
            Prompt = request.Prompt,
            ReferenceAssetId = request.ReferenceAssetId,
            Width = request.Width ?? 800,
            Height = request.Height ?? 600,
            Format = request.Format ?? "png",
            Quality = request.Quality ?? 90,
        }, ct);

        return File(result.ImageData, result.MimeType, result.FileName);
    }
}

/// <summary>POST /api/generate リクエストボディ</summary>
public record GeneratePromptApiRequest(
    string Prompt,
    Guid? ReferenceAssetId,
    int? Width,
    int? Height,
    string? Format,
    int? Quality);
