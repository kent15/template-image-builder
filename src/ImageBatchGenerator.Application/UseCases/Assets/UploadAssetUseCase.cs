using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Application.UseCases.Assets;

/// <summary>
/// 素材アップロードユースケース
/// 単体ファイルまたはZIPを受け取り、素材ライブラリに登録する
/// </summary>
public class UploadAssetUseCase
{
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
        // TODO: Phase 2で実装
        // 1. MIMEタイプ・拡張子・ファイルサイズバリデーション（最大50MB）
        // 2. ストレージに保存
        // 3. Assetエンティティ作成・DB保存
        // 4. AssetのIdを返却
        throw new NotImplementedException();
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
        // TODO: Phase 2で実装
        // 1. ZIPファイルを展開
        // 2. 各エントリについてMIMEタイプバリデーション
        // 3. ストレージに一括保存
        // 4. Assetエンティティ一括作成・DB保存
        // 5. AssetのIdリストを返却
        throw new NotImplementedException();
    }
}
