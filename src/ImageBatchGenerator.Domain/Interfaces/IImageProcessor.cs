using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.ValueObjects;

namespace ImageBatchGenerator.Domain.Interfaces;

/// <summary>
/// 画像生成プロセッサインターフェース（Infrastructure層のSkiaSharpで実装）
/// </summary>
public interface IImageProcessor
{
    // TODO: メソッド実装（Phase 3）

    /// <summary>
    /// テンプレートに差し替えデータを適用して画像を生成する
    /// </summary>
    /// <param name="template">使用するテンプレート</param>
    /// <param name="inputData">CSVから読み込んだ差し替えデータ（列名→値）</param>
    /// <param name="format">出力フォーマット</param>
    /// <param name="outputQuality">JPEG品質（0-100）。PNGの場合は無視</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>生成画像のバイト配列</returns>
    Task<byte[]> GenerateAsync(
        Template template,
        IReadOnlyDictionary<string, string> inputData,
        OutputFormat format,
        int outputQuality = 90,
        CancellationToken ct = default);

    /// <summary>
    /// サンプルプレビュー用に先頭N件を生成する
    /// </summary>
    Task<IReadOnlyList<byte[]>> GeneratePreviewAsync(
        Template template,
        IReadOnlyList<IReadOnlyDictionary<string, string>> inputDataList,
        OutputFormat format,
        CancellationToken ct = default);
}
