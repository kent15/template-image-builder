namespace ImageBatchGenerator.Domain.ValueObjects;

/// <summary>
/// 画像出力フォーマット値オブジェクト
/// </summary>
public enum OutputFormat
{
    /// <summary>PNG形式（可逆圧縮）</summary>
    Png,

    /// <summary>JPEG形式（非可逆圧縮・品質設定あり）</summary>
    Jpeg,

    /// <summary>WebP形式（高圧縮率・Webブラウザ向け）</summary>
    WebP,
}
