using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ImageBatchGenerator.Web.Middleware;

/// <summary>
/// グローバル例外ハンドリングミドルウェア
/// 未処理例外をキャッチして適切なHTTPレスポンスに変換しSerilogに記録する
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // TODO: Phase 5で実装
        // try
        // {
        //     await _next(context);
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, "Unhandled exception occurred");
        //     await HandleExceptionAsync(context, ex);
        // }
        await _next(context);
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // TODO: Phase 5で実装
        // 例外種別に応じてHTTPステータスコードを決定する
        // - NotFoundException → 404
        // - ValidationException → 400
        // - その他 → 500
        throw new NotImplementedException();
    }
}
