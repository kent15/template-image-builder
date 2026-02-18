using System.Text.Json;
using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.Interfaces;
using ImageBatchGenerator.Domain.ValueObjects;
using ImageBatchGenerator.Infrastructure.ImageProcessing.Models;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageBatchGenerator.Infrastructure.ImageProcessing;

/// <summary>
/// SixLabors.ImageSharp を使用したテンプレート型画像合成プロセッサ
/// Template.LayerConfigJson に基づき、背景・商品画像・テキストをレイヤー順に合成する
/// すべての中間 Image は using で即時破棄しメモリ効率を確保する
/// </summary>
public class ImageSharpProcessor : IImageProcessor
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly ILogger<ImageSharpProcessor> _logger;

    public ImageSharpProcessor(ILogger<ImageSharpProcessor> logger)
    {
        _logger = logger;
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Public API
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// テンプレートに差し替えデータを適用して1枚の画像を生成する
    /// inputData: レイヤーID → 値（テキスト or 画像ファイルパス）
    /// </summary>
    public async Task<byte[]> GenerateAsync(
        Template template,
        IReadOnlyDictionary<string, string> inputData,
        OutputFormat format,
        int outputQuality = 90,
        CancellationToken ct = default)
    {
        var config = ParseLayerConfig(template.LayerConfigJson);

        // Rgba32 キャンバス（PNG 透過対応）
        using var canvas = new Image<Rgba32>(template.Width, template.Height, Color.Transparent);

        foreach (var layer in config.Layers)
        {
            ct.ThrowIfCancellationRequested();

            // inputData にレイヤー ID が無ければスキップ
            if (!inputData.TryGetValue(layer.Id, out var value) || string.IsNullOrEmpty(value))
                continue;

            try
            {
                switch (layer.Type.ToLowerInvariant())
                {
                    case "background":
                        DrawBackgroundLayer(canvas, layer, value);
                        break;
                    case "productimage":
                        DrawImageLayer(canvas, layer, value);
                        break;
                    case "text":
                        DrawTextLayer(canvas, layer, value);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Layer {LayerId} ({Type}) の描画に失敗しました。スキップします。",
                    layer.Id, layer.Type);
            }
        }

        return await EncodeAsync(canvas, format, outputQuality, ct);
    }

    /// <summary>
    /// プレビュー用に複数件を順次生成する
    /// </summary>
    public async Task<IReadOnlyList<byte[]>> GeneratePreviewAsync(
        Template template,
        IReadOnlyList<IReadOnlyDictionary<string, string>> inputDataList,
        OutputFormat format,
        CancellationToken ct = default)
    {
        var results = new List<byte[]>(inputDataList.Count);
        foreach (var inputData in inputDataList)
        {
            ct.ThrowIfCancellationRequested();
            results.Add(await GenerateAsync(template, inputData, format, ct: ct));
        }
        return results;
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Background Layer — ストレッチでキャンバス全面に配置
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private static void DrawBackgroundLayer(Image<Rgba32> canvas, LayerDefinition layer, string imagePath)
    {
        if (!File.Exists(imagePath)) return;

        using var overlay = Image.Load<Rgba32>(imagePath);

        overlay.Mutate(ctx => ctx.Resize(new ResizeOptions
        {
            Size = new Size(layer.Width, layer.Height),
            Mode = ResizeMode.Stretch,
        }));

        canvas.Mutate(ctx => ctx.DrawImage(overlay, new Point(layer.X, layer.Y), layer.Opacity));
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Product Image Layer — contain/cover/fill + 中央配置
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private static void DrawImageLayer(Image<Rgba32> canvas, LayerDefinition layer, string imagePath)
    {
        if (!File.Exists(imagePath)) return;

        using var overlay = Image.Load<Rgba32>(imagePath);

        var resizeMode = (layer.Fit?.ToLowerInvariant()) switch
        {
            "cover" => ResizeMode.Crop,
            "fill" => ResizeMode.Stretch,
            _ => ResizeMode.Max, // contain: アスペクト比維持で枠内に収める
        };

        overlay.Mutate(ctx => ctx.Resize(new ResizeOptions
        {
            Size = new Size(layer.Width, layer.Height),
            Mode = resizeMode,
        }));

        // リサイズ後の実サイズでレイヤー領域内に中央配置
        var offsetX = layer.X + (layer.Width - overlay.Width) / 2;
        var offsetY = layer.Y + (layer.Height - overlay.Height) / 2;

        canvas.Mutate(ctx => ctx.DrawImage(overlay, new Point(offsetX, offsetY), layer.Opacity));
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Text Layer — フォント指定・自動縮小・アラインメント
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private static void DrawTextLayer(Image<Rgba32> canvas, LayerDefinition layer, string text)
    {
        var family = ResolveFontFamily(layer.FontFamily);
        var style = ToFontStyle(layer.Bold, layer.Italic);
        var requestedSize = layer.FontSize > 0 ? layer.FontSize : 24f;

        // レイヤー領域に収まるフォントサイズを二分探索で決定
        var font = ScaleToFit(text, family, style, requestedSize, layer.Width, layer.Height);
        var color = ParseColor(layer.FontColor);

        var textOptions = new RichTextOptions(font)
        {
            Origin = new PointF(layer.X, layer.Y),
            WrappingLength = layer.Width,
            LineSpacing = layer.LineSpacing,
            HorizontalAlignment = ParseHAlign(layer.HorizontalAlign),
            VerticalAlignment = ParseVAlign(layer.VerticalAlign),
        };

        canvas.Mutate(ctx => ctx.DrawText(textOptions, text, new SolidBrush(color), null));
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Encoding — PNG 透過 / JPEG / WebP
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private static async Task<byte[]> EncodeAsync(
        Image<Rgba32> image, OutputFormat format, int quality, CancellationToken ct)
    {
        using var ms = new MemoryStream();

        switch (format)
        {
            case OutputFormat.Jpeg:
                await image.SaveAsJpegAsync(ms, new JpegEncoder { Quality = quality }, ct);
                break;
            case OutputFormat.WebP:
                await image.SaveAsWebpAsync(ms, new WebpEncoder { Quality = quality }, ct);
                break;
            default: // PNG（透過保持）
                await image.SaveAsPngAsync(ms, new PngEncoder
                {
                    ColorType = PngColorType.RgbWithAlpha,
                    CompressionLevel = PngCompressionLevel.BestSpeed, // バッチ処理用に速度優先
                }, ct);
                break;
        }

        return ms.ToArray();
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Font Resolution
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private static FontFamily ResolveFontFamily(string? familyName)
    {
        // 指定フォントを検索
        if (!string.IsNullOrEmpty(familyName) && SystemFonts.TryGet(familyName, out var family))
            return family;

        // フォールバック: システムに存在する最初のフォント
        var fallback = SystemFonts.Families.FirstOrDefault();
        if (fallback.Name is not null)
            return fallback;

        throw new InvalidOperationException(
            "システムにフォントが見つかりません。ttf/otf ファイルをインストールしてください。");
    }

    private static FontStyle ToFontStyle(bool bold, bool italic) =>
        (bold, italic) switch
        {
            (true, true) => FontStyle.BoldItalic,
            (true, false) => FontStyle.Bold,
            (false, true) => FontStyle.Italic,
            _ => FontStyle.Regular,
        };

    /// <summary>
    /// テキストがレイヤー境界内に収まるフォントサイズを二分探索で決定する
    /// O(log n) — 最大で約15回のイテレーション
    /// </summary>
    private static Font ScaleToFit(
        string text, FontFamily family, FontStyle style,
        float maxSize, float maxWidth, float maxHeight)
    {
        const float MinSize = 6f;
        const float Tolerance = 0.5f;

        float lo = MinSize, hi = maxSize;
        var bestFont = family.CreateFont(MinSize, style);

        while (hi - lo > Tolerance)
        {
            var mid = (lo + hi) / 2f;
            var testFont = family.CreateFont(mid, style);
            var opts = new TextOptions(testFont) { WrappingLength = maxWidth };
            var bounds = TextMeasurer.MeasureSize(text, opts);

            if (bounds.Width <= maxWidth && bounds.Height <= maxHeight)
            {
                bestFont = testFont;
                lo = mid;
            }
            else
            {
                hi = mid;
            }
        }

        return bestFont;
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  Parse Helpers
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private static TemplateLayerConfig ParseLayerConfig(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new TemplateLayerConfig();

        return JsonSerializer.Deserialize<TemplateLayerConfig>(json, JsonOpts)
            ?? new TemplateLayerConfig();
    }

    private static Color ParseColor(string? hex)
        => !string.IsNullOrEmpty(hex) ? Color.ParseHex(hex) : Color.Black;

    private static HorizontalAlignment ParseHAlign(string? align) =>
        align?.ToLowerInvariant() switch
        {
            "center" => HorizontalAlignment.Center,
            "right" => HorizontalAlignment.Right,
            _ => HorizontalAlignment.Left,
        };

    private static VerticalAlignment ParseVAlign(string? align) =>
        align?.ToLowerInvariant() switch
        {
            "center" => VerticalAlignment.Center,
            "bottom" => VerticalAlignment.Bottom,
            _ => VerticalAlignment.Top,
        };
}
