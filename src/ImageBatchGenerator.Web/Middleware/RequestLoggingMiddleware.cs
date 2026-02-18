using Microsoft.Extensions.Logging;

namespace ImageBatchGenerator.Web.Middleware;

/// <summary>
/// リクエストロギングミドルウェア
/// HTTPリクエスト・レスポンスをSerilogで構造化ログ記録する
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // TODO: Phase 5で実装
        // Serilog.AspNetCoreのUseSerilogRequestLoggingで代替可能なため要否を検討する
        await _next(context);
    }
}
