using ImageBatchGenerator.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageBatchGenerator.Infrastructure.ImageProcessing;

/// <summary>
/// プロンプト画像生成のスタブ実装（AI API 未接続フェーズ用）。
/// プロンプトテキストをキャンバス上に描画したプレースホルダー画像を返す。
/// 参照素材が指定された場合はその画像を背景として利用する。
///
/// 実際の AI API（DALL-E / Stable Diffusion 等）を接続する場合は
/// IPromptImageGenerator を別クラスで実装して DI で差し替えること。
/// </summary>
public class StubPromptImageGenerator : IPromptImageGenerator
{
    private readonly IStorageService _storageService;
    private readonly ILogger<StubPromptImageGenerator> _logger;

    public StubPromptImageGenerator(
        IStorageService storageService,
        ILogger<StubPromptImageGenerator> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<byte[]> GenerateAsync(
        string prompt,
        string? referenceAssetStoragePath,
        int width,
        int height,
        CancellationToken ct = default)
    {
        Image<Rgba32> canvas;

        if (!string.IsNullOrEmpty(referenceAssetStoragePath) &&
            await _storageService.ExistsAsync(referenceAssetStoragePath, ct))
        {
            // 参照素材を読み込んでリサイズ
            var stream = await _storageService.ReadAsync(referenceAssetStoragePath, ct);
            canvas = Image.Load<Rgba32>(stream);
            canvas.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Crop,
            }));
            // テキストを見やすくするための半透明オーバーレイ
            canvas.Mutate(ctx => ctx.Fill(
                new SolidBrush(Color.FromRgba(0, 0, 0, 150)),
                new Rectangle(0, 0, canvas.Width, canvas.Height)));

            _logger.LogInformation("プロンプト生成: 参照素材を使用 ({Path})", referenceAssetStoragePath);
        }
        else
        {
            // インディゴ系グラデーション背景を生成
            canvas = new Image<Rgba32>(width, height);
            canvas.Mutate(ctx =>
            {
                for (int y = 0; y < height; y++)
                {
                    var t = (float)y / height;
                    var r = (byte)(79 + (124 - 79) * t);
                    var g = (byte)(70 + (58 - 70) * t);
                    var b = (byte)(229 + (237 - 229) * t);
                    ctx.Fill(Color.FromRgb(r, g, b), new Rectangle(0, y, width, 1));
                }
            });
        }

        using (canvas)
        {
            var family = TryGetFontFamily();

            if (family.HasValue)
            {
                // プロンプトテキストを中央に描画
                var promptFont = ScaleToFit(
                    prompt, family.Value, FontStyle.Regular, 52f,
                    canvas.Width - 100, canvas.Height - 140);

                canvas.Mutate(ctx => ctx.DrawText(
                    new RichTextOptions(promptFont)
                    {
                        Origin = new PointF(canvas.Width / 2f, canvas.Height / 2f),
                        WrappingLength = canvas.Width - 100,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                    prompt,
                    Color.White));

                // スタブ注記（右下）
                var labelFont = family.Value.CreateFont(13f, FontStyle.Regular);
                canvas.Mutate(ctx => ctx.DrawText(
                    new RichTextOptions(labelFont)
                    {
                        Origin = new PointF(canvas.Width - 12, canvas.Height - 10),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Bottom,
                    },
                    "Stub — AI API 接続で実画像に切り替わります",
                    Color.FromRgba(255, 255, 255, 140)));
            }

            using var ms = new MemoryStream();
            await canvas.SaveAsPngAsync(ms, new PngEncoder(), ct);
            return ms.ToArray();
        }
    }

    // ── Helpers ────────────────────────────────────────────────

    private static FontFamily? TryGetFontFamily()
    {
        var families = SystemFonts.Families.ToList();
        if (families.Count == 0) return null;

        // 日本語フォント優先
        var preferred = families.FirstOrDefault(f =>
            f.Name.Contains("Gothic", StringComparison.OrdinalIgnoreCase) ||
            f.Name.Contains("Meiryo", StringComparison.OrdinalIgnoreCase) ||
            f.Name.Contains("Noto", StringComparison.OrdinalIgnoreCase));

        return preferred.Name is not null ? preferred : families[0];
    }

    private static Font ScaleToFit(
        string text, FontFamily family, FontStyle style,
        float maxSize, float maxWidth, float maxHeight)
    {
        const float MinSize = 10f;
        const float Tolerance = 0.5f;

        float lo = MinSize, hi = maxSize;
        var best = family.CreateFont(MinSize, style);

        while (hi - lo > Tolerance)
        {
            var mid = (lo + hi) / 2f;
            var testFont = family.CreateFont(mid, style);
            var opts = new TextOptions(testFont) { WrappingLength = maxWidth };
            var bounds = TextMeasurer.MeasureSize(text, opts);

            if (bounds.Width <= maxWidth && bounds.Height <= maxHeight)
            {
                best = testFont;
                lo = mid;
            }
            else
            {
                hi = mid;
            }
        }

        return best;
    }
}
