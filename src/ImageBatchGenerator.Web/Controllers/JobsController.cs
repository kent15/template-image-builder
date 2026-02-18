using ImageBatchGenerator.Application.DTOs;
using ImageBatchGenerator.Application.UseCases.Jobs;
using Microsoft.AspNetCore.Mvc;

namespace ImageBatchGenerator.Web.Controllers;

/// <summary>
/// ジョブ管理APIコントローラー
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly CreateJobUseCase _createJobUseCase;
    private readonly StartJobUseCase _startJobUseCase;
    private readonly CancelJobUseCase _cancelJobUseCase;
    private readonly RetryJobUseCase _retryJobUseCase;
    private readonly GetJobProgressUseCase _getJobProgressUseCase;

    public JobsController(
        CreateJobUseCase createJobUseCase,
        StartJobUseCase startJobUseCase,
        CancelJobUseCase cancelJobUseCase,
        RetryJobUseCase retryJobUseCase,
        GetJobProgressUseCase getJobProgressUseCase)
    {
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
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }

    /// <summary>ジョブ詳細を取得する</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }

    /// <summary>ジョブを作成する（ウィザード完了時に呼ばれる）</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobRequest request, CancellationToken ct)
    {
        // TODO: Phase 2で実装
        // var result = await _createJobUseCase.ExecuteAsync(request, ct);
        // return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        throw new NotImplementedException();
    }

    /// <summary>ジョブをキューに投入して実行を開始する</summary>
    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id, CancellationToken ct)
    {
        // TODO: Phase 3で実装
        throw new NotImplementedException();
    }

    /// <summary>実行中ジョブをキャンセルする</summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        // TODO: Phase 3で実装
        throw new NotImplementedException();
    }

    /// <summary>エラー分のみ再実行する</summary>
    [HttpPost("{id:guid}/retry")]
    public async Task<IActionResult> Retry(Guid id, CancellationToken ct)
    {
        // TODO: Phase 3で実装
        throw new NotImplementedException();
    }

    /// <summary>ジョブの現在の進捗を取得する（ポーリングフォールバック用）</summary>
    [HttpGet("{id:guid}/progress")]
    public async Task<IActionResult> GetProgress(Guid id, CancellationToken ct)
    {
        // TODO: Phase 3で実装
        throw new NotImplementedException();
    }

    /// <summary>生成画像をZIPで一括ダウンロードする</summary>
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        // TODO: Phase 4で実装
        throw new NotImplementedException();
    }
}
