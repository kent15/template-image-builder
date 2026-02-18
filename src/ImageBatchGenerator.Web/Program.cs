using ImageBatchGenerator.Application.Interfaces;
using ImageBatchGenerator.Application.Orchestration;
using ImageBatchGenerator.Application.UseCases.Assets;
using ImageBatchGenerator.Application.UseCases.Jobs;
using ImageBatchGenerator.Application.UseCases.Templates;
using ImageBatchGenerator.Domain.Interfaces;
using ImageBatchGenerator.Infrastructure.ImageProcessing;
using ImageBatchGenerator.Infrastructure.Notifications;
using ImageBatchGenerator.Infrastructure.Persistence;
using ImageBatchGenerator.Infrastructure.Persistence.Repositories;
using ImageBatchGenerator.Infrastructure.Queue;
using ImageBatchGenerator.Infrastructure.Storage;
using ImageBatchGenerator.Web.BackgroundServices;
using ImageBatchGenerator.Web.Hubs;
using ImageBatchGenerator.Web.Middleware;
using Microsoft.EntityFrameworkCore;
using Serilog;

// TODO: Phase 1でSerilogの設定を追加する
// Log.Logger = new LoggerConfiguration()
//     .ReadFrom.Configuration(builder.Configuration)
//     .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// TODO: Phase 1でSerilogをホストロガーとして設定する
// builder.Host.UseSerilog();

// ── DB（EF Core + SQL Server） ──────────────────────────────
// TODO: Phase 1で有効化する
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// ── リポジトリ（Domain Interface → Infrastructure実装） ──────
// TODO: Phase 1〜2で有効化する
// builder.Services.AddScoped<IJobRepository, JobRepository>();
// builder.Services.AddScoped<IJobItemRepository, JobItemRepository>();
// builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();
// builder.Services.AddScoped<IAssetRepository, AssetRepository>();

// ── ストレージ・画像処理・通知（Domain Interface → Infrastructure実装） ──
// TODO: Phase 2〜3で有効化する
// builder.Services.AddScoped<IStorageService, LocalStorageService>();
// builder.Services.AddScoped<IImageProcessor, SkiaSharpImageProcessor>();
// builder.Services.AddSingleton<IJobQueue, InMemoryJobQueue>();
// builder.Services.AddScoped<IProgressNotifier, SignalRProgressNotifier>();

// ── アプリケーション層（UseCases・Orchestration） ────────────
// TODO: Phase 2〜3で有効化する
// builder.Services.AddScoped<CreateJobUseCase>();
// builder.Services.AddScoped<StartJobUseCase>();
// builder.Services.AddScoped<CancelJobUseCase>();
// builder.Services.AddScoped<RetryJobUseCase>();
// builder.Services.AddScoped<GetJobProgressUseCase>();
// builder.Services.AddScoped<RegisterTemplateUseCase>();
// builder.Services.AddScoped<UploadAssetUseCase>();
// builder.Services.AddScoped<JobOrchestrator>();
// builder.Services.AddScoped<BatchCoordinator>();

// ── SignalR ──────────────────────────────────────────────────
// TODO: Phase 3で有効化する
// builder.Services.AddSignalR();

// ── バックグラウンドサービス ────────────────────────────────
// TODO: Phase 3で有効化する
// builder.Services.AddHostedService<JobQueueWorker>();

// ── ヘルスチェック ────────────────────────────────────────────
// TODO: Phase 1で有効化する
// builder.Services.AddHealthChecks()
//     .AddDbContextCheck<AppDbContext>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// TODO: Phase 5でExceptionHandlingMiddlewareを有効化する
// app.UseMiddleware<ExceptionHandlingMiddleware>();

// TODO: Phase 5でRequestLoggingMiddlewareを有効化する（またはUseSerilogRequestLogging）
// app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// TODO: Phase 1でヘルスチェックエンドポイントを有効化する
// app.MapHealthChecks("/health");

// TODO: Phase 3でSignalRハブを有効化する
// app.MapHub<ProgressHub>("/hubs/progress");

app.Run();
