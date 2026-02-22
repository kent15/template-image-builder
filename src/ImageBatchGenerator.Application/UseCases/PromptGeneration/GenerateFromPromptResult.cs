namespace ImageBatchGenerator.Application.UseCases.PromptGeneration;

/// <summary>
/// プロンプト画像生成ユースケースの結果
/// </summary>
public class GenerateFromPromptResult
{
    /// <summary>生成ファイル名</summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>MIMEタイプ（image/png など）</summary>
    public string MimeType { get; init; } = "image/png";

    /// <summary>生成画像のバイト配列</summary>
    public byte[] ImageData { get; init; } = [];
}
