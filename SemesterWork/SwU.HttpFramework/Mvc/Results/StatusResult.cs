using System.Net;
using SwU.HttpFramework.Server;

namespace SwU.HttpFramework.Mvc.Results;

public class StatusResult : IActionResult
{
    private readonly int _statusCode;
    private readonly string _message;
    private readonly string _title;

    public StatusResult(int statusCode, string message, string? title = null)
    {
        _statusCode = statusCode;
        _message = message ?? string.Empty;
        _title = title ?? GetDefaultTitle(statusCode);
    }

    public async Task ExecuteAsync(HttpListenerContext context)
    {
        string html = ErrorPageBuilder.Build(_statusCode, _title, _message);
        await HtmlResult.WriteResponseSafelyAsync(context, _statusCode, "text/html; charset=utf-8", html);
    }

    private static string GetDefaultTitle(int statusCode)
    {
        return statusCode switch
        {
            400 => "Bad request",
            401 => "Authorization required",
            403 => "Access denied",
            404 => "Page not found",
            405 => "Method not allowed",
            500 => "Internal server error",
            _ => "Error"
        };
    }
}
