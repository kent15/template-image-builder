using ImageBatchGenerator.Application.DTOs;
using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Application.UseCases.Jobs;

/// <summary>
/// ジョブ進捗取得ユースケース
/// SignalR が使えない場合のポーリングフォールバック用
/// </summary>
public class GetJobProgressUseCase
{
    private readonly IJobRepository _jobRepository;

    public GetJobProgressUseCase(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    /// <summary>
    /// ジョブの現在の進捗 DTO を返す
    /// </summary>
    public async Task<JobProgressDto> ExecuteAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, ct)
            ?? throw new InvalidOperationException($"Job not found: {jobId}");

        var processedCount = job.ProcessedCount;
        var progressRate = job.TotalCount > 0
            ? (double)processedCount / job.TotalCount
            : 0.0;

        double? itemsPerSecond = null;
        double? estimatedRemainingSeconds = null;

        if (job.StartedAt.HasValue && processedCount > 0)
        {
            var elapsedSeconds = (DateTimeOffset.UtcNow - job.StartedAt.Value).TotalSeconds;
            if (elapsedSeconds > 0)
            {
                itemsPerSecond = processedCount / elapsedSeconds;
                var remaining = job.TotalCount - processedCount;
                estimatedRemainingSeconds = remaining / itemsPerSecond;
            }
        }

        return new JobProgressDto
        {
            JobId = job.Id,
            ProcessedCount = processedCount,
            TotalCount = job.TotalCount,
            SuccessCount = job.SuccessCount,
            WarningCount = job.WarningCount,
            ErrorCount = job.ErrorCount,
            ProgressRate = progressRate,
            ItemsPerSecond = itemsPerSecond,
            EstimatedRemainingSeconds = estimatedRemainingSeconds,
        };
    }
}
