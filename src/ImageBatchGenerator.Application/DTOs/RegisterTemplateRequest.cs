namespace ImageBatchGenerator.Application.DTOs;

/// <summary>
/// テンプレート登録リクエスト DTO
/// Swagger から JSON で送信するためファイルアップロード不要（Phase 1 検証用）
/// </summary>
public record RegisterTemplateRequest
{
    /// <summary>テンプレート名</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>説明（省略可）</summary>
    public string? Description { get; init; }

    /// <summary>出力画像の幅（px）</summary>
    public int Width { get; init; } = 800;

    /// <summary>出力画像の高さ（px）</summary>
    public int Height { get; init; } = 600;

    /// <summary>
    /// レイヤー構成 JSON
    /// 例:
    /// {"layers":[
    ///   {"id":"title","type":"text","x":50,"y":100,"width":700,"height":100,
    ///    "fontSize":48,"fontColor":"#333333","horizontalAlign":"center"},
    ///   {"id":"subtitle","type":"text","x":50,"y":250,"width":700,"height":60,
    ///    "fontSize":24,"fontColor":"#666666","horizontalAlign":"center"}
    /// ]}
    /// </summary>
    public string LayerConfigJson { get; init; } = "{\"layers\":[]}";
}
