using System.Net;
using System.Text;

namespace SwU.HttpFramework.Mvc.Results;

public class HtmlResult : IActionResult
{
    private readonly string _html;
    private readonly int _statusCode;

    public HtmlResult(string html, int statusCode = 200)
    {
        _html = html ?? string.Empty;
        _statusCode = statusCode;
    }

    public async Task ExecuteAsync(HttpListenerContext context)
    {
        await WriteResponseSafelyAsync(context, _statusCode, "text/html; charset=utf-8", _html);
    }

    internal static async Task WriteResponseSafelyAsync(
        HttpListenerContext context,
        int statusCode,
        string contentType,
        string content)
    {
        try
        {
            HttpListenerResponse response = context.Response;
            byte[] buffer = Encoding.UTF8.GetBytes(content);

            response.StatusCode = statusCode;
            response.ContentType = contentType;
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = buffer.Length;

            if (!string.Equals(context.Request.HttpMethod, "HEAD", StringComparison.OrdinalIgnoreCase))
            {
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }

            response.Close();
        }
        catch (HttpListenerException)
        {
            // Клиент оборвал соединение
        }
        catch (ObjectDisposedException)
        {
            // Ответ уже закрыт
        }
    }
}
