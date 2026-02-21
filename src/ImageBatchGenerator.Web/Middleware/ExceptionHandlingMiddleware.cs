using System.Net;
using System.Text.Json;

namespace ImageBatchGenerator.Web.Middleware;

/// <summary>
/// グローバル例外ハンドリングミドルウェア
/// 未処理例外をキャッチして適切なHTTPレスポンスに変換し記録する
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
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while processing {Method} {Path}",
                context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            NotImplementedException => (HttpStatusCode.NotImplemented, "この機能は未実装です。"),
            OperationCanceledException => (HttpStatusCode.BadRequest, "リクエストがキャンセルされました。"),
            _ => (HttpStatusCode.InternalServerError, "サーバー内部でエラーが発生しました。"),
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new
        {
            error = message,
            statusCode = (int)statusCode,
        });

        await context.Response.WriteAsync(body);
    }
}
