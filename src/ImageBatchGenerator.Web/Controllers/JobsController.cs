using ImageBatchGenerator.Application.DTOs;
using ImageBatchGenerator.Application.UseCases.Jobs;
using ImageBatchGenerator.Domain.Entities;
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
    private readonly ITemplateRepository _templateRepository;
    private readonly IStorageService _storageService;
    private readonly CreateJobUseCase _createJobUseCase;
    private readonly StartJobUseCase _startJobUseCase;
    private readonly CancelJobUseCase _cancelJobUseCase;
    private readonly RetryJobUseCase _retryJobUseCase;
    private readonly GetJobProgressUseCase _getJobProgressUseCase;

    public JobsController(
        IJobRepository jobRepository,
        ITemplateRepository templateRepository,
        IStorageService storageService,
        CreateJobUseCase createJobUseCase,
        StartJobUseCase startJobUseCase,
        CancelJobUseCase cancelJobUseCase,
        RetryJobUseCase retryJobUseCase,
        GetJobProgressUseCase getJobProgressUseCase)
    {
        _jobRepository = jobRepository;
        _templateRepository = templateRepository;
        _storageService = storageService;
        _createJobUseCase = createJobUseCase;
        _startJobUseCase = startJobUseCase;
        _cancelJobUseCase = cancelJobUseCase;
        _retryJobUseCase = retryJobUseCase;
        _getJobProgressUseCase = getJobProgressUseCase;
    }

    /// <summary>ジョブ一覧を取得する（テンプレート名付き）</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var jobs = await _jobRepository.GetAllAsync(ct);
        var templates = await _templateRepository.GetAllActiveAsync(ct);
        var templateMap = templates.ToDictionary(t => t.Id, t => t.Name);
        var dtos = jobs.Select(j => EnrichWithTemplateName(j, templateMap));
        return Ok(dtos);
    }

    /// <summary>ジョブ詳細を取得する</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var job = await _jobRepository.GetByIdAsync(id, ct);
        if (job is null) return NotFound();
        var template = await _templateRepository.GetByIdAsync(job.TemplateId, ct);
        var dto = job.ToDto() with { TemplateName = template?.Name ?? "" };
        return Ok(dto);
    }

    private static JobDto EnrichWithTemplateName(Job job, Dictionary<Guid, string> templateMap)
    {
        var dto = job.ToDto();
        return templateMap.TryGetValue(job.TemplateId, out var name)
            ? dto with { TemplateName = name }
            : dto;
    }

    /// <summary>
    /// ジョブを作成する
    /// InputRows を使うと実際の入力データでジョブを作成できる
    /// 例: "inputRows": [{"title":"商品A","subtitle":"¥1,000"},{"title":"商品B","subtitle":"¥2,000"}]
    /// </summary>
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

    /// <summary>ジョブの現在の進捗を取得する（ポーリング用）</summary>
    [HttpGet("{id:guid}/progress")]
    public async Task<IActionResult> GetProgress(Guid id, CancellationToken ct)
    {
        var progress = await _getJobProgressUseCase.ExecuteAsync(id, ct);
        return Ok(progress);
    }

    /// <summary>生成画像を ZIP で一括ダウンロードする</summary>
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var job = await _jobRepository.GetByIdAsync(id, ct);
        if (job is null) return NotFound();

        var stream = await _storageService.CreateZipArchiveAsync(id, ct);
        return File(stream, "application/zip", $"job_{id}.zip");
    }
}
