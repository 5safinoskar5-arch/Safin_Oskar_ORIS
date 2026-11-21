using System.Net;
using System.Text;

namespace SwU.HttpFramework.Server;

internal static class ErrorPageBuilder
{
    // Шаблон страницы ошибки. Плейсхолдеры заменяются уже экранированными значениями,
    // поэтому фигурные скобки в CSS не конфликтуют с интерполяцией строк.
    private const string PageTemplate = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <title>__STATUS__ — __TITLE__</title>
    <style>
        * { box-sizing: border-box; }
        body {
            margin: 0;
            min-height: 100vh;
            background: #f4f3f1;
            color: #1c1b1a;
            font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Arial, Helvetica, sans-serif;
            font-size: 15px;
            line-height: 1.5;
        }
        .error-header {
            height: 56px;
            display: flex;
            align-items: center;
            padding: 0 24px;
            background: #000000;
            color: #ffffff;
            font-size: 14px;
            font-weight: 600;
            letter-spacing: 0.2em;
            text-transform: uppercase;
        }
        .error-main {
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 64px 16px;
        }
        .error-card {
            width: 100%;
            max-width: 520px;
            padding: 32px;
            background: #ffffff;
            border: 1px solid #e4e2df;
            border-radius: 8px;
            text-align: center;
        }
        .error-code {
            margin: 0 0 8px;
            font-size: 56px;
            font-weight: 700;
            line-height: 1;
        }
        .error-card h1 {
            margin: 0 0 12px;
            font-size: 20px;
            font-weight: 700;
        }
        .error-message {
            margin: 0 0 24px;
            color: #706c68;
        }
        .error-button {
            display: inline-block;
            padding: 10px 20px;
            border-radius: 4px;
            background: #000000;
            color: #ffffff;
            font-size: 14px;
            font-weight: 600;
            text-decoration: none;
        }
        .error-button:hover { background: #333333; }
    </style>
</head>
<body>
    <header class=""error-header"">UNLIMITED</header>
    <main class=""error-main"">
        <div class=""error-card"">
            <p class=""error-code"">__STATUS__</p>
            <h1>__TITLE__</h1>
            <p class=""error-message"">__MESSAGE__</p>
            <a class=""error-button"" href=""/"">Back to All Cards</a>
        </div>
    </main>
</body>
</html>";

    public static string Build(int statusCode, string title, string message)
    {
        string safeTitle = WebUtility.HtmlEncode(title);
        string safeMessage = WebUtility.HtmlEncode(message);

        return PageTemplate
            .Replace("__STATUS__", statusCode.ToString(), StringComparison.Ordinal)
            .Replace("__TITLE__", safeTitle, StringComparison.Ordinal)
            .Replace("__MESSAGE__", safeMessage, StringComparison.Ordinal);
    }

    public static async Task WriteHtmlSafelyAsync(HttpListenerContext context, int statusCode, string html)
    {
        try
        {
            HttpListenerResponse response = context.Response;
            byte[] buffer = Encoding.UTF8.GetBytes(html);

            response.StatusCode = statusCode;
            response.ContentType = "text/html; charset=utf-8";
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
            // Клиент оборвал соединение во время отправки ответа
        }
        catch (ObjectDisposedException)
        {
            // Поток ответа уже закрыт
        }
        catch (InvalidOperationException)
        {
            // Заголовки или ответ уже отправлены
        }
    }
}
