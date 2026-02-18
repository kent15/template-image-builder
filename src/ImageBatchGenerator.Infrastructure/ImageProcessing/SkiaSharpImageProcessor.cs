using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.Interfaces;
using ImageBatchGenerator.Domain.ValueObjects;

namespace ImageBatchGenerator.Infrastructure.ImageProcessing;

/// <summary>
/// SkiaSharp（MIT License）を使用した画像生成プロセッサ
/// テキスト・背景・商品画像のレイヤー差し替えを行う
/// </summary>
public class SkiaSharpImageProcessor : IImageProcessor
{
    // TODO: Phase 3でSkiaSharpの参照を追加
    // using SkiaSharp;

    public async Task<byte[]> GenerateAsync(
        Template template,
        IReadOnlyDictionary<string, string> inputData,
        OutputFormat format,
        int outputQuality = 90,
        CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        // 1. テンプレートファイルを読み込む（SVG/JSON解析）
        // 2. SKSurface/SKCanvasを生成（template.Width x template.Height）
        // 3. 各レイヤーを処理する:
        //    - 背景レイヤー: SKBitmap.Decode → DrawBitmap
        //    - 商品画像レイヤー: SKBitmap.Decode → 自動トリミング → DrawBitmap
        //    - テキストレイヤー: SKPaint → DrawText（オーバーフロー時は自動縮小）
        // 4. SKData.ToArray()で出力フォーマットにエンコード
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<byte[]>> GeneratePreviewAsync(
        Template template,
        IReadOnlyList<IReadOnlyDictionary<string, string>> inputDataList,
        OutputFormat format,
        CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        // inputDataListの各要素についてGenerateAsync()を呼び出してリストで返す
        throw new NotImplementedException();
    }

    /// <summary>テキストがレイヤー境界内に収まるようにフォントサイズを自動調整する</summary>
    private float MeasureAndScaleText(string text, float maxWidth, float maxHeight, object paint)
    {
        // TODO: Phase 3で実装
        // SKPaint.MeasureText()でテキスト幅を計算し、必要に応じてTextSizeを縮小する
        throw new NotImplementedException();
    }
}
