using ImageBatchGenerator.Application.DTOs;
using ImageBatchGenerator.Domain.Interfaces;

namespace ImageBatchGenerator.Application.UseCases.Jobs;

/// <summary>
/// ジョブ進捗取得ユースケース
/// ポーリングフォールバック用のREST APIエンドポイントから呼ばれる
/// </summary>
public class GetJobProgressUseCase
{
    private readonly IJobRepository _jobRepository;

    public GetJobProgressUseCase(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    /// <summary>
    /// ジョブの現在の進捗DTOを返す
    /// </summary>
    public async Task<JobProgressDto> ExecuteAsync(Guid jobId, CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        // 1. ジョブ取得・存在確認
        // 2. 進捗率・処理速度・推定残り時間を計算
        // 3. JobProgressDtoに変換して返却
        throw new NotImplementedException();
    }
}
