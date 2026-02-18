using ImageBatchGenerator.Application.DTOs;
using ImageBatchGenerator.Application.UseCases.Jobs;
using ImageBatchGenerator.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ImageBatchGenerator.Web.Controllers;

/// <summary>
/// ジョブ管理 API コントローラー
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobRepository _jobRepository;
    private readonly CreateJobUseCase _createJobUseCase;
    private readonly StartJobUseCase _startJobUseCase;
    private readonly CancelJobUseCase _cancelJobUseCase;
    private readonly RetryJobUseCase _retryJobUseCase;
    private readonly GetJobProgressUseCase _getJobProgressUseCase;

    public JobsController(
        IJobRepository jobRepository,
        CreateJobUseCase createJobUseCase,
        StartJobUseCase startJobUseCase,
        CancelJobUseCase cancelJobUseCase,
        RetryJobUseCase retryJobUseCase,
        GetJobProgressUseCase getJobProgressUseCase)
    {
        _jobRepository = jobRepository;
        _createJobUseCase = createJobUseCase;
        _startJobUseCase = startJobUseCase;
        _cancelJobUseCase = cancelJobUseCase;
        _retryJobUseCase = retryJobUseCase;
        _getJobProgressUseCase = getJobProgressUseCase;
    }

    /// <summary>ジョブ一覧を取得する</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var jobs = await _jobRepository.GetAllAsync(ct);
        var dtos = jobs.Select(j => j.ToDto());
        return Ok(dtos);
    }

    /// <summary>ジョブ詳細を取得する</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var job = await _jobRepository.GetByIdAsync(id, ct);
        if (job is null) return NotFound();
        return Ok(job.ToDto());
    }

    /// <summary>ジョブを作成する（ウィザード完了時に呼ばれる）</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobRequest request, CancellationToken ct)
    {
        var result = await _createJobUseCase.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>ジョブをキューに投入して実行を開始する</summary>
    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id, CancellationToken ct)
    {
        await _startJobUseCase.ExecuteAsync(id, ct);
        return NoContent();
    }

    /// <summary>実行中ジョブをキャンセルする</summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await _cancelJobUseCase.ExecuteAsync(id, ct);
        return NoContent();
    }

    /// <summary>エラー分のみ再実行する</summary>
    [HttpPost("{id:guid}/retry")]
    public async Task<IActionResult> Retry(Guid id, CancellationToken ct)
    {
        await _retryJobUseCase.ExecuteAsync(id, ct);
        return NoContent();
    }

    /// <summary>ジョブの現在の進捗を取得する（ポーリングフォールバック用）</summary>
    [HttpGet("{id:guid}/progress")]
    public async Task<IActionResult> GetProgress(Guid id, CancellationToken ct)
    {
        var progress = await _getJobProgressUseCase.ExecuteAsync(id, ct);
        return Ok(progress);
    }

    /// <summary>生成画像を ZIP で一括ダウンロードする（Phase 4 で実装）</summary>
    [HttpGet("{id:guid}/download")]
    public IActionResult Download(Guid id)
        => StatusCode(501, new { message = "ダウンロード機能は Phase 4 で実装予定です。" });
}
