using System.Net;
using System.Text;

public class HttpServer
{
    private readonly HttpListener _listener = new();
    private readonly string _htmlPath;

    public HttpServer(string url, string htmlPath)
    {
        _listener.Prefixes.Add(url);
        _htmlPath = htmlPath;
    }

    public async Task Start()
    {
        _listener.Start();
        Console.WriteLine("Сервер запущен.");

        while (_listener.IsListening)
        {
            HttpListenerContext context;

            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (HttpListenerException) when (!_listener.IsListening)
            {
                break;
            }
            catch (ObjectDisposedException) when (!_listener.IsListening)
            {
                break;
            }

            try
            {
                byte[] html = await File.ReadAllBytesAsync(_htmlPath);

                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/html; charset=utf-8";
                context.Response.ContentLength64 = html.Length;

                await context.Response.OutputStream.WriteAsync(html);
            }
            catch (FileNotFoundException)
            {
                byte[] message = Encoding.UTF8.GetBytes("Файл search-engine.html не найден.");

                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/plain; charset=utf-8";
                context.Response.ContentLength64 = message.Length;

                await context.Response.OutputStream.WriteAsync(message);
            }
            finally
            {
                context.Response.Close();
            }
        }

        Console.WriteLine("Сервер остановлен.");
    }

    public void Stop()
    {
        if (_listener.IsListening)
            _listener.Stop();
    }
}
