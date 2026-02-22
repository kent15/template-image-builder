namespace ImageBatchGenerator.Domain.Interfaces;

/// <summary>
/// テキストプロンプトから画像を生成するサービスインターフェース。
/// 現フェーズはスタブ実装（ImageSharp）、将来的にAI API実装へ差し替え可能。
/// </summary>
public interface IPromptImageGenerator
{
    /// <summary>
    /// テキストプロンプトから画像を生成する。
    /// </summary>
    /// <param name="prompt">生成指示テキスト（例: "犬が散歩している画像"）</param>
    /// <param name="referenceAssetStoragePath">参照素材のストレージパス（null = 参照なし）</param>
    /// <param name="width">出力幅（px）</param>
    /// <param name="height">出力高（px）</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>生成画像のバイト配列（PNG）</returns>
    Task<byte[]> GenerateAsync(
        string prompt,
        string? referenceAssetStoragePath,
        int width,
        int height,
        CancellationToken ct = default);
}
