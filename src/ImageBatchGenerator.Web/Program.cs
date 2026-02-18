using ImageBatchGenerator.Application.Interfaces;
using ImageBatchGenerator.Application.Orchestration;
using ImageBatchGenerator.Application.UseCases.Jobs;
using ImageBatchGenerator.Domain.Interfaces;
using ImageBatchGenerator.Infrastructure.ImageProcessing;
using ImageBatchGenerator.Infrastructure.Jobs;
using ImageBatchGenerator.Infrastructure.Persistence.Repositories;
using ImageBatchGenerator.Infrastructure.Queue;
using ImageBatchGenerator.Web.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

// ── CORS（開発時はすべて許可） ────────────────────────────────
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()));

// ── リポジトリ（インメモリ実装） ─────────────────────────────
// Phase 2以降は EF Core 実装へ差し替える
// builder.Services.AddScoped<IJobRepository, JobRepository>();
// builder.Services.AddScoped<IJobItemRepository, JobItemRepository>();
builder.Services.AddSingleton<IJobRepository, InMemoryJobRepository>();
builder.Services.AddSingleton<IJobItemRepository, InMemoryJobItemRepository>();

// ── キュー（インメモリ実装） ──────────────────────────────────
builder.Services.AddSingleton<IJobQueue, InMemoryJobQueue>();

// ── キャンセルレジストリ ──────────────────────────────────────
builder.Services.AddSingleton<IJobCancellationRegistry, InMemoryJobCancellationRegistry>();

// ── 画像処理（ImageSharp） ──────────────────────────────────
builder.Services.AddScoped<IImageProcessor, ImageSharpProcessor>();

// ── ユースケース ────────────────────────────────────────────
builder.Services.AddScoped<CreateJobUseCase>();
builder.Services.AddScoped<StartJobUseCase>();
builder.Services.AddScoped<CancelJobUseCase>();
builder.Services.AddScoped<RetryJobUseCase>();
builder.Services.AddScoped<GetJobProgressUseCase>();

// ── オーケストレーター（Scoped: ジョブ単位のスコープで解決される） ──
builder.Services.AddScoped<JobOrchestrator>();

// ── バックグラウンドワーカー ─────────────────────────────────
builder.Services.AddHostedService<JobQueueWorker>();

// ── ヘルスチェック ──────────────────────────────────────────
builder.Services.AddHealthChecks();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// TODO: Phase 5 — ExceptionHandlingMiddleware, RequestLoggingMiddleware
// TODO: Phase 3 — SignalR hub（ProgressHub）

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
