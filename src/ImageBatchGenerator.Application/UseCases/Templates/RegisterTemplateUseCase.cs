using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Application.UseCases.Templates;

/// <summary>
/// テンプレート登録ユースケース
/// SVG/JSONファイルをストレージに保存してDBに登録する
/// </summary>
public class RegisterTemplateUseCase
{
    private readonly ITemplateRepository _templateRepository;
    private readonly IStorageService _storageService;
    private readonly IImageProcessor _imageProcessor;

    public RegisterTemplateUseCase(
        ITemplateRepository templateRepository,
        IStorageService storageService,
        IImageProcessor imageProcessor)
    {
        _templateRepository = templateRepository;
        _storageService = storageService;
        _imageProcessor = imageProcessor;
    }

    /// <summary>
    /// テンプレートを登録する
    /// </summary>
    /// <param name="name">テンプレート名</param>
    /// <param name="description">説明</param>
    /// <param name="fileStream">テンプレートファイルストリーム（SVG/JSON）</param>
    /// <param name="fileName">アップロードファイル名</param>
    /// <param name="width">出力幅</param>
    /// <param name="height">出力高さ</param>
    /// <param name="layerConfigJson">レイヤー構成JSON</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>登録されたテンプレートID</returns>
    public async Task<Guid> ExecuteAsync(
        string name,
        string? description,
        Stream fileStream,
        string fileName,
        int width,
        int height,
        string layerConfigJson,
        CancellationToken ct = default)
    {
        // TODO: Phase 2で実装
        // 1. MIMEタイプ・拡張子バリデーション
        // 2. ストレージにテンプレートファイルを保存
        // 3. サムネイル生成・保存
        // 4. Templateエンティティ作成・DB保存
        // 5. 生成したTemplateのIdを返却
        throw new NotImplementedException();
    }
}
