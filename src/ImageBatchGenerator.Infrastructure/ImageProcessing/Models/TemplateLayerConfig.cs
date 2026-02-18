using System.Text.Json.Serialization;

namespace ImageBatchGenerator.Infrastructure.ImageProcessing.Models;

/// <summary>
/// Template.LayerConfigJson のデシリアライズモデル
/// </summary>
public record TemplateLayerConfig
{
    [JsonPropertyName("layers")]
    public List<LayerDefinition> Layers { get; init; } = new();
}

/// <summary>
/// レイヤー定義（背景・商品画像・テキスト共通）
/// </summary>
public record LayerDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>レイヤー種別: Background / ProductImage / Text</summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    // ── 位置・サイズ ──────────────────────────────────────────
    [JsonPropertyName("x")]
    public int X { get; init; }

    [JsonPropertyName("y")]
    public int Y { get; init; }

    [JsonPropertyName("width")]
    public int Width { get; init; }

    [JsonPropertyName("height")]
    public int Height { get; init; }

    // ── 共通 ──────────────────────────────────────────────────
    /// <summary>不透明度 0.0〜1.0（デフォルト 1.0）</summary>
    [JsonPropertyName("opacity")]
    public float Opacity { get; init; } = 1.0f;

    // ── 画像レイヤー用 ───────────────────────────────────────
    /// <summary>フィットモード: contain（デフォルト） / cover / fill</summary>
    [JsonPropertyName("fit")]
    public string? Fit { get; init; }

    // ── テキストレイヤー用 ───────────────────────────────────
    [JsonPropertyName("fontFamily")]
    public string? FontFamily { get; init; }

    [JsonPropertyName("fontSize")]
    public float FontSize { get; init; } = 24f;

    /// <summary>16進数カラーコード（例: "#FF0000"）</summary>
    [JsonPropertyName("fontColor")]
    public string? FontColor { get; init; }

    [JsonPropertyName("bold")]
    public bool Bold { get; init; }

    [JsonPropertyName("italic")]
    public bool Italic { get; init; }

    /// <summary>水平揃え: left（デフォルト） / center / right</summary>
    [JsonPropertyName("horizontalAlign")]
    public string? HorizontalAlign { get; init; }

    /// <summary>垂直揃え: top（デフォルト） / center / bottom</summary>
    [JsonPropertyName("verticalAlign")]
    public string? VerticalAlign { get; init; }

    /// <summary>行間（ラインスペーシング倍率。デフォルト 1.0）</summary>
    [JsonPropertyName("lineSpacing")]
    public float LineSpacing { get; init; } = 1.0f;
}
