using ImageBatchGenerator.Application.Interfaces;
using ImageBatchGenerator.Application.UseCases.Jobs;
using ImageBatchGenerator.Domain.Interfaces;
using ImageBatchGenerator.Infrastructure.Persistence.Repositories;
using ImageBatchGenerator.Infrastructure.Queue;

var builder = WebApplication.CreateBuilder(args);

// ── CORS（開発時はすべて許可） ────────────────────────────────
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()));

// ── リポジトリ（インメモリ実装） ─────────────────────────────
// Phase 1: 以下インメモリ版を使用。Phase 2以降は EF Core 実装へ差し替え。
// builder.Services.AddScoped<IJobRepository, JobRepository>();
// builder.Services.AddScoped<IJobItemRepository, JobItemRepository>();
builder.Services.AddSingleton<IJobRepository, InMemoryJobRepository>();
builder.Services.AddSingleton<IJobItemRepository, InMemoryJobItemRepository>();

// ── キュー（インメモリ実装） ──────────────────────────────────
builder.Services.AddSingleton<IJobQueue, InMemoryJobQueue>();

// ── ユースケース ────────────────────────────────────────────
builder.Services.AddScoped<CreateJobUseCase>();
builder.Services.AddScoped<StartJobUseCase>();
builder.Services.AddScoped<CancelJobUseCase>();
builder.Services.AddScoped<RetryJobUseCase>();
builder.Services.AddScoped<GetJobProgressUseCase>();

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
// TODO: Phase 3 — SignalR hub, JobQueueWorker

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
