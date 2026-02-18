using ImageBatchGenerator.Application.DTOs;
using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Application.UseCases.Templates;

/// <summary>
/// テンプレート登録ユースケース（Phase 1: JSON定義のみ、ファイルアップロードなし）
/// Phase 2 以降でファイルアップロード・サムネイル生成を追加する
/// </summary>
public class RegisterTemplateUseCase
{
    private readonly ITemplateRepository _templateRepository;

    public RegisterTemplateUseCase(ITemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    /// <summary>
    /// テンプレートを登録して DTO を返す
    /// </summary>
    public async Task<TemplateDto> ExecuteAsync(
        RegisterTemplateRequest request,
        CancellationToken ct = default)
    {
        var template = Template.Create(
            name: request.Name,
            description: request.Description,
            width: request.Width,
            height: request.Height,
            layerConfigJson: request.LayerConfigJson);

        await _templateRepository.AddAsync(template, ct);

        return template.ToDto();
    }
}
