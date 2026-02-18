using ImageBatchGenerator.Domain.Entities;
using ImageBatchGenerator.Domain.ValueObjects;

namespace ImageBatchGenerator.Domain.Interfaces;

/// <summary>
/// Jobリポジトリインターフェース（Infrastructure層で実装）
/// </summary>
public interface IJobRepository
{
    // TODO: メソッド実装（Phase 1〜2）

    Task<Job?> GetByIdAsync(Guid jobId, CancellationToken ct = default);

    Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken ct = default);

    Task<IReadOnlyList<Job>> GetByStatusAsync(JobStatus status, CancellationToken ct = default);

    /// <summary>キュー待機中のジョブを作成日時順に取得する</summary>
    Task<Job?> DequeueNextAsync(CancellationToken ct = default);

    Task AddAsync(Job job, CancellationToken ct = default);

    Task UpdateAsync(Job job, CancellationToken ct = default);

    /// <summary>チェックポイント（LastProcessedIndex）をDB更新する</summary>
    Task UpdateCheckpointAsync(Guid jobId, int lastProcessedIndex, CancellationToken ct = default);
}
