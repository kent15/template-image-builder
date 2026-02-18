using ImageBatchGenerator.Application.UseCases.Templates;
using Microsoft.AspNetCore.Mvc;

namespace ImageBatchGenerator.Web.Controllers;

/// <summary>
/// テンプレート管理APIコントローラー
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly RegisterTemplateUseCase _registerTemplateUseCase;

    public TemplatesController(RegisterTemplateUseCase registerTemplateUseCase)
    {
        _registerTemplateUseCase = registerTemplateUseCase;
    }

    /// <summary>テンプレート一覧を取得する</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }

    /// <summary>テンプレート詳細を取得する</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }

    /// <summary>テンプレートを登録する（ファイルアップロード）</summary>
    [HttpPost]
    public async Task<IActionResult> Register([FromForm] IFormFile file, [FromForm] string name, CancellationToken ct)
    {
        // TODO: Phase 2で実装
        // MIMEタイプ・拡張子チェック（SVG/JSON限定）
        throw new NotImplementedException();
    }

    /// <summary>テンプレートを更新する</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CancellationToken ct)
    {
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }

    /// <summary>テンプレートを論理削除する</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }

    /// <summary>テンプレートのプレビュー画像を取得する</summary>
    [HttpGet("{id:guid}/preview")]
    public async Task<IActionResult> Preview(Guid id, CancellationToken ct)
    {
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }
}
