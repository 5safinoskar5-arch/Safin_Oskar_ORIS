using System.Net;
using SwU.HttpFramework.Pipeline;

namespace SwU.HttpFramework.Server;

public class WebServer
{
    private readonly ServerConfig _config;
    private readonly RequestHandler _firstHandler;
    private readonly HttpListener _listener = new();

    public WebServer(ServerConfig config, RequestHandler firstHandler)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _firstHandler = firstHandler ?? throw new ArgumentNullException(nameof(firstHandler));
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        string prefix = _config.Prefix.EndsWith('/') ? _config.Prefix : _config.Prefix + "/";
        _listener.Prefixes.Add(prefix);
        _listener.Start();

        using CancellationTokenRegistration registration = cancellationToken.Register(StopListenerSafely);

        try
        {
            while (!cancellationToken.IsCancellationRequested && _listener.IsListening)
            {
                HttpListenerContext context;
                try
                {
                    context = await _listener.GetContextAsync();
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }

                _ = Task.Run(() => ProcessRequestAsync(context));
            }
        }
        finally
        {
            StopListenerSafely();
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        try
        {
            await _firstHandler.HandleAsync(context);
        }
        catch (HttpListenerException)
        {
            // Клиент разорвал соединение — завершаем обработку тихо
        }
        catch (ObjectDisposedException)
        {
            // Соединение или ответ уже освобождены
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[WebServer] Ошибка при обработке запроса: {ex}");
            string html = ErrorPageBuilder.Build(
                500,
                "Internal server error",
                "An unexpected error occurred while processing the request.");
            await ErrorPageBuilder.WriteHtmlSafelyAsync(context, 500, html);
        }
        finally
        {
            CloseResponseSafely(context);
        }
    }

    private void StopListenerSafely()
    {
        try
        {
            if (_listener.IsListening)
            {
                _listener.Stop();
            }

            _listener.Close();
        }
        catch (ObjectDisposedException)
        {
        }
        catch (HttpListenerException)
        {
        }
    }

    private static void CloseResponseSafely(HttpListenerContext context)
    {
        try
        {
            context.Response.Close();
        }
        catch (HttpListenerException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
    }
}
