using ImageBatchGenerator.Application.DTOs;
using ImageBatchGenerator.Application.UseCases.Templates;
using ImageBatchGenerator.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ImageBatchGenerator.Web.Controllers;

/// <summary>
/// テンプレート管理 API コントローラー
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateRepository _templateRepository;
    private readonly RegisterTemplateUseCase _registerTemplateUseCase;

    public TemplatesController(
        ITemplateRepository templateRepository,
        RegisterTemplateUseCase registerTemplateUseCase)
    {
        _templateRepository = templateRepository;
        _registerTemplateUseCase = registerTemplateUseCase;
    }

    /// <summary>テンプレート一覧を取得する</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var templates = await _templateRepository.GetAllActiveAsync(ct);
        return Ok(templates.Select(t => t.ToDto()));
    }

    /// <summary>テンプレート詳細を取得する</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var template = await _templateRepository.GetByIdAsync(id, ct);
        if (template is null) return NotFound();
        return Ok(template.ToDto());
    }

    /// <summary>
    /// テンプレートを登録する
    /// LayerConfigJson の例（テキスト2レイヤー）:
    /// {"layers":[
    ///   {"id":"title","type":"text","x":50,"y":100,"width":700,"height":100,"fontSize":48,"fontColor":"#333333","horizontalAlign":"center"},
    ///   {"id":"subtitle","type":"text","x":50,"y":250,"width":700,"height":60,"fontSize":24,"fontColor":"#666666","horizontalAlign":"center"}
    /// ]}
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Register(
        [FromBody] RegisterTemplateRequest request,
        CancellationToken ct)
    {
        var dto = await _registerTemplateUseCase.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    /// <summary>テンプレートを論理削除する</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var template = await _templateRepository.GetByIdAsync(id, ct);
        if (template is null) return NotFound();
        await _templateRepository.SoftDeleteAsync(id, ct);
        return NoContent();
    }

    // TODO: PUT（バージョン更新）、プレビュー生成は Phase 2 で実装
}
