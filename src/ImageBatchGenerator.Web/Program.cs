using ImageBatchGenerator.Application.Interfaces;
using ImageBatchGenerator.Application.Options;
using ImageBatchGenerator.Application.Orchestration;
using ImageBatchGenerator.Application.UseCases.Assets;
using ImageBatchGenerator.Application.UseCases.Jobs;
using ImageBatchGenerator.Application.UseCases.PromptGeneration;
using ImageBatchGenerator.Application.UseCases.Templates;
using ImageBatchGenerator.Domain.Interfaces;
using ImageBatchGenerator.Infrastructure.ImageProcessing;
using ImageBatchGenerator.Infrastructure.Jobs;
using ImageBatchGenerator.Infrastructure.Notifications;
using ImageBatchGenerator.Infrastructure.Persistence.Repositories;
using ImageBatchGenerator.Infrastructure.Queue;
using ImageBatchGenerator.Infrastructure.Storage;
using ImageBatchGenerator.Web.BackgroundServices;
using ImageBatchGenerator.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── バッチ設定 ────────────────────────────────────────────────
builder.Services.Configure<BatchSettings>(
    builder.Configuration.GetSection(BatchSettings.SectionName));

// ── CORS（開発時はすべて許可） ────────────────────────────────
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()));

// ── リポジトリ（インメモリ実装） ─────────────────────────────
// Phase 2以降は EF Core 実装へ差し替える
builder.Services.AddSingleton<IJobRepository, InMemoryJobRepository>();
builder.Services.AddSingleton<IJobItemRepository, InMemoryJobItemRepository>();
builder.Services.AddSingleton<ITemplateRepository, InMemoryTemplateRepository>();
builder.Services.AddSingleton<IAssetRepository, InMemoryAssetRepository>();

// ── キュー（インメモリ実装） ──────────────────────────────────
builder.Services.AddSingleton<IJobQueue, InMemoryJobQueue>();

// ── キャンセルレジストリ ──────────────────────────────────────
builder.Services.AddSingleton<IJobCancellationRegistry, InMemoryJobCancellationRegistry>();

// ── ストレージ ────────────────────────────────────────────────
builder.Services.AddSingleton<IStorageService, LocalStorageService>();

// ── 進捗通知（Phase 3 で SignalRProgressNotifier に差し替え） ─
builder.Services.AddSingleton<IProgressNotifier, NullProgressNotifier>();

// ── 画像処理（ImageSharp） ──────────────────────────────────
builder.Services.AddScoped<IImageProcessor, ImageSharpProcessor>();

// ── プロンプト画像生成（スタブ実装 / AI API接続時は差し替え） ──
builder.Services.AddScoped<IPromptImageGenerator, StubPromptImageGenerator>();

// ── ユースケース ────────────────────────────────────────────
builder.Services.AddScoped<RegisterTemplateUseCase>();
builder.Services.AddScoped<UploadAssetUseCase>();
builder.Services.AddScoped<CreateJobUseCase>();
builder.Services.AddScoped<StartJobUseCase>();
builder.Services.AddScoped<CancelJobUseCase>();
builder.Services.AddScoped<RetryJobUseCase>();
builder.Services.AddScoped<GetJobProgressUseCase>();
builder.Services.AddScoped<GenerateFromPromptUseCase>();

// ── バッチ処理コーディネーター ────────────────────────────────
builder.Services.AddScoped<BatchCoordinator>();

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

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors();

// 静的ファイル配信（wwwroot/index.html をフロントエンドとして配信）
app.UseDefaultFiles();
app.UseStaticFiles();

// TODO: Phase 3 — SignalR hub（ProgressHub）

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
