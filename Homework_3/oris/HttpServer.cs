using System.Net;
using System.Text;

public class HttpServer
{
    private readonly HttpListener _listener = new();
    private readonly string _rootDirectory;
    private readonly string _defaultDocument;
    private readonly StringComparison _pathComparison;
    private Task? _listeningTask;
    private bool _running;

    public HttpServer(ServerSettings settings)
    {
        _rootDirectory = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(settings.RootDirectory, AppContext.BaseDirectory));
        _defaultDocument = settings.DefaultDocument;
        _pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        if (!Directory.Exists(_rootDirectory))
            throw new DirectoryNotFoundException($"Нет папки {_rootDirectory}");

        Uri url = new(settings.Url);
        _listener.Prefixes.Add(settings.Url);

        if (url.Host == "localhost")
            _listener.Prefixes.Add($"http://127.0.0.1:{url.Port}/");
        else if (url.Host == "127.0.0.1")
            _listener.Prefixes.Add($"http://localhost:{url.Port}/");
    }

    public void Start()
    {
        _listener.Start();
        _running = true;
        _listeningTask = ListenAsync();
        Console.WriteLine("Сервер запущен.");
    }

    public async Task Stop()
    {
        _running = false;

        try
        {
            if (_listener.IsListening)
                _listener.Stop();

            if (_listeningTask != null)
                await _listeningTask;
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Ошибка остановки: {exception.Message}");
        }
        finally
        {
            _listener.Close();
        }

        Console.WriteLine("Сервер остановлен.");
    }

    private async Task ListenAsync()
    {
        while (_running)
        {
            HttpListenerContext context;

            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (Exception exception)
            {
                if (!_running)
                    break;

                Console.WriteLine($"Ошибка получения запроса: {exception.Message}");

                if (!_listener.IsListening)
                    break;

                await Task.Delay(100);
                continue;
            }

            await HandleRequestAsync(context);
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        try
        {
            if (context.Request.HttpMethod != "GET" && context.Request.HttpMethod != "HEAD")
            {
                context.Response.Headers["Allow"] = "GET, HEAD";
                await SendErrorAsync(context, 405, "Method Not Allowed");
                return;
            }

            string urlPath = Uri.UnescapeDataString(context.Request.Url?.AbsolutePath ?? "/");

            if (urlPath.Contains('\\') || urlPath.Contains(':') || urlPath.Contains('\0'))
            {
                await SendErrorAsync(context, 404, "Not Found");
                return;
            }

            string relativePath = urlPath.TrimStart('/');

            if (relativePath == "")
                relativePath = _defaultDocument;
            else if (relativePath.EndsWith('/'))
                relativePath += "index.html";

            string filePath = Path.GetFullPath(Path.Combine(_rootDirectory,
                relativePath.Replace('/', Path.DirectorySeparatorChar)));
            string rootPrefix = _rootDirectory + Path.DirectorySeparatorChar;

            if (!filePath.StartsWith(rootPrefix, _pathComparison) || !File.Exists(filePath))
            {
                await SendErrorAsync(context, 404, "Not Found");
                return;
            }

            string currentPath = _rootDirectory;

            foreach (string part in Path.GetRelativePath(_rootDirectory, filePath)
                .Split(Path.DirectorySeparatorChar))
            {
                currentPath = Path.Combine(currentPath, part);

                if ((File.GetAttributes(currentPath) & FileAttributes.ReparsePoint) != 0)
                {
                    await SendErrorAsync(context, 404, "Not Found");
                    return;
                }
            }

            byte[] data = await File.ReadAllBytesAsync(filePath);
            await SendResponseAsync(context, 200, GetContentType(filePath), data);
            Console.WriteLine($"200 {context.Request.Url?.AbsolutePath}");
        }
        catch (FileNotFoundException)
        {
            await TrySendErrorAsync(context, 404, "Not Found");
        }
        catch (DirectoryNotFoundException)
        {
            await TrySendErrorAsync(context, 404, "Not Found");
        }
        catch (UnauthorizedAccessException exception)
        {
            Console.WriteLine($"Нет доступа к файлу: {exception.Message}");
            await TrySendErrorAsync(context, 403, "Forbidden");
        }
        catch (ArgumentException)
        {
            await TrySendErrorAsync(context, 400, "Bad Request");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Ошибка обработки запроса: {exception.Message}");
            await TrySendErrorAsync(context, 500, "Internal Server Error");
        }
        finally
        {
            try
            {
                context.Response.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Ответ уже закрыт или соединение прервано: {exception.Message}");
            }
        }
    }

    private static async Task SendResponseAsync(HttpListenerContext context,
        int statusCode, string contentType, byte[] data)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = contentType;
        context.Response.ContentLength64 = data.Length;

        if (context.Request.HttpMethod != "HEAD")
            await context.Response.OutputStream.WriteAsync(data);
    }

    private static async Task SendErrorAsync(HttpListenerContext context, int statusCode, string title)
    {
        string html = $"<!DOCTYPE html><html lang=\"en\"><meta charset=\"UTF-8\">"
            + $"<title>{statusCode} {title}</title><body><h1>{statusCode} {title}</h1>"
            + "<p><a href=\"/\">Back to home</a></p></body></html>";

        await SendResponseAsync(context, statusCode, "text/html; charset=utf-8",
            Encoding.UTF8.GetBytes(html));
        Console.WriteLine($"{statusCode} {context.Request.Url?.AbsolutePath}");
    }

    private static async Task TrySendErrorAsync(HttpListenerContext context, int statusCode, string title)
    {
        try
        {
            await SendErrorAsync(context, statusCode, title);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Не удалось отправить ответ: {exception.Message}");
        }
    }

    private static string GetContentType(string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".html" or ".htm" => "text/html; charset=utf-8",
            ".css" => "text/css; charset=utf-8",
            ".js" => "text/javascript; charset=utf-8",
            ".json" => "application/json; charset=utf-8",
            ".txt" => "text/plain; charset=utf-8",
            ".svg" => "image/svg+xml",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".ico" => "image/x-icon",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}
