namespace ImageBatchGenerator.Application.Options;

/// <summary>
/// バッチ処理設定（appsettings.json の "BatchSettings" セクションにバインド）
/// </summary>
public class BatchSettings
{
    public const string SectionName = "BatchSettings";

    /// <summary>1ジョブ内の最大並列処理数（画像生成スレッド数）</summary>
    public int MaxDegreeOfParallelism { get; set; } = 2;

    /// <summary>同時実行可能なジョブ数</summary>
    public int MaxConcurrentJobs { get; set; } = 1;

    /// <summary>エラー時の最大再試行回数</summary>
    public int RetryLimit { get; set; } = 3;
}
