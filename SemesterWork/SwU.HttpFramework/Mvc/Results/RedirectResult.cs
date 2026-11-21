using System.Net;
using System.Text;

namespace SwU.HttpFramework.Mvc.Results;

public class RedirectResult : IActionResult
{
    private readonly string _location;
    private readonly int _statusCode;

    public RedirectResult(string location, int statusCode = 302)
    {
        _location = string.IsNullOrWhiteSpace(location) ? "/" : location;
        _statusCode = statusCode;
    }

    public Task ExecuteAsync(HttpListenerContext context)
    {
        try
        {
            HttpListenerResponse response = context.Response;
            response.StatusCode = _statusCode;
            response.RedirectLocation = _location;
            response.ContentType = "text/plain; charset=utf-8";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = 0;
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

        return Task.CompletedTask;
    }
}
