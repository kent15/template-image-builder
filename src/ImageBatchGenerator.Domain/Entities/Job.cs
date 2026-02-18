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

    /// <summary>処理済み件数（成功 + 警告 + エラー + スキップ）</summary>
    public int ProcessedCount => SuccessCount + WarningCount + ErrorCount + SkippedCount;

    private Job() { }

    /// <summary>ジョブを新規作成する</summary>
    public static Job Create(
        string name,
        Guid templateId,
        string mappingRulesJson,
        string outputSettingsJson,
        int totalCount = 0,
        string? csvOriginalFileName = null,
        string? csvStoragePath = null,
        string? createdBy = null)
    {
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Name = name,
            TemplateId = templateId,
            Status = JobStatus.Created,
            MappingRulesJson = mappingRulesJson,
            OutputSettingsJson = outputSettingsJson,
            TotalCount = totalCount,
            CsvOriginalFileName = csvOriginalFileName,
            CsvStoragePath = csvStoragePath,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        return job;
    }

    /// <summary>ジョブをキューに追加する</summary>
    public void Enqueue()
    {
        Status = JobStatus.Queued;
        QueuedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>ジョブの実行を開始する</summary>
    public void Start()
    {
        Status = JobStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>ジョブを完了状態にする（エラー・警告があれば CompletedWithWarning）</summary>
    public void Complete()
    {
        Status = (ErrorCount > 0 || WarningCount > 0)
            ? JobStatus.CompletedWithWarning
            : JobStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>ジョブをキャンセルする</summary>
    public void Cancel()
    {
        Status = JobStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>チェックポイントを更新する</summary>
    public void UpdateCheckpoint(int lastProcessedIndex)
    {
        LastProcessedIndex = lastProcessedIndex;
    }

    /// <summary>進捗カウンタを加算する</summary>
    public void IncrementProgress(bool success, bool warning, bool error)
    {
        if (success) SuccessCount++;
        if (warning) WarningCount++;
        if (error) ErrorCount++;
    }

    /// <summary>エラー再実行のためにカウンタをリセットして再キューする</summary>
    public void ResetForRetry()
    {
        ErrorCount = 0;
        RetryCount++;
        Enqueue();
    }
}
