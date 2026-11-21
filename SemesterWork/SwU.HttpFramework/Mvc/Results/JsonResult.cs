using System.Net;
using System.Text.Json;

namespace SwU.HttpFramework.Mvc.Results;

public class JsonResult : IActionResult
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly object? _data;
    private readonly int _statusCode;

    public JsonResult(object? data, int statusCode = 200)
    {
        _data = data;
        _statusCode = statusCode;
    }

    public async Task ExecuteAsync(HttpListenerContext context)
    {
        string json = JsonSerializer.Serialize(_data, SerializerOptions);
        await HtmlResult.WriteResponseSafelyAsync(context, _statusCode, "application/json; charset=utf-8", json);
    }
}
