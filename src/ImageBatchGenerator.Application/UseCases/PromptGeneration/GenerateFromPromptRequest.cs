namespace ImageBatchGenerator.Application.UseCases.PromptGeneration;

/// <summary>
/// プロンプト画像生成ユースケースへのリクエスト
/// </summary>
public class GenerateFromPromptRequest
{
    /// <summary>生成指示テキスト（例: "犬が散歩している画像を生成して"）</summary>
    public string Prompt { get; init; } = string.Empty;

    /// <summary>参照素材ID（null = 参照なし）</summary>
    public Guid? ReferenceAssetId { get; init; }

    /// <summary>出力幅（px）デフォルト 800</summary>
    public int Width { get; init; } = 800;

    /// <summary>出力高（px）デフォルト 600</summary>
    public int Height { get; init; } = 600;

    /// <summary>出力フォーマット（png / jpeg / webp）</summary>
    public string Format { get; init; } = "png";

    /// <summary>JPEG/WebP 品質 0-100</summary>
    public int Quality { get; init; } = 90;
}
