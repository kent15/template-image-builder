using ImageBatchGenerator.Domain.ValueObjects;

namespace ImageBatchGenerator.Domain.Entities;

/// <summary>
/// バッチジョブ集約ルート
/// </summary>
public class Job
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid TemplateId { get; private set; }
    public JobStatus Status { get; private set; }
    public string MappingRulesJson { get; private set; } = string.Empty;
    public string OutputSettingsJson { get; private set; } = string.Empty;
    public string? CsvOriginalFileName { get; private set; }
    public string? CsvStoragePath { get; private set; }
    public int TotalCount { get; private set; }
    public int SuccessCount { get; private set; }
    public int WarningCount { get; private set; }
    public int ErrorCount { get; private set; }
    public int SkippedCount { get; private set; }

    /// <summary>チェックポイント（途中再開用）</summary>
    public int LastProcessedIndex { get; private set; }

    public int RetryCount { get; private set; }

    /// <summary>再実行元ジョブID（エラー再実行時に設定）</summary>
    public Guid? OriginalJobId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? QueuedAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? CreatedBy { get; private set; }

    public Template? Template { get; private set; }
    public IReadOnlyCollection<JobItem> Items { get; private set; } = new List<JobItem>();

    // TODO: ドメインロジック実装（Phase 2〜3）

    /// <summary>ジョブをキューに追加する</summary>
    public void Enqueue()
    {
        // TODO: Statusをキュー済みに変更し、QueuedAtを設定する
        throw new NotImplementedException();
    }

    /// <summary>ジョブの実行を開始する</summary>
    public void Start()
    {
        // TODO: Statusを実行中に変更し、StartedAtを設定する
        throw new NotImplementedException();
    }

    /// <summary>ジョブを完了状態にする</summary>
    public void Complete()
    {
        // TODO: 成功・警告・エラー件数に応じてStatusを決定し、CompletedAtを設定する
        throw new NotImplementedException();
    }

    /// <summary>ジョブをキャンセルする</summary>
    public void Cancel()
    {
        // TODO: Statusをキャンセル済みに変更する
        throw new NotImplementedException();
    }

    /// <summary>チェックポイントを更新する</summary>
    public void UpdateCheckpoint(int lastProcessedIndex)
    {
        // TODO: LastProcessedIndexを更新する
        throw new NotImplementedException();
    }

    /// <summary>進捗カウンタを加算する</summary>
    public void IncrementProgress(bool success, bool warning, bool error)
    {
        // TODO: SuccessCount / WarningCount / ErrorCountを加算する
        throw new NotImplementedException();
    }
}
