using System.Net;
using System.Text;
using SwU.HttpFramework.Mvc.Results;

namespace SwU.HttpFramework.Mvc;

public abstract class BaseController
{
    public HttpListenerContext Context { get; set; } = null!;
    public int MaxBodyBytes { get; set; } = 262144;

    private Dictionary<string, string>? _cachedQuery;

    protected string? GetQuery(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        _cachedQuery ??= ParseQueryString(Context.Request.Url?.Query);
        return _cachedQuery.TryGetValue(key, out string? value) ? value : null;
    }

    protected int? GetQueryInt(string key)
    {
        string? raw = GetQuery(key);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return int.TryParse(raw.Trim(), out int value) ? value : null;
    }

    protected async Task<Dictionary<string, string>> ReadFormAsync()
    {
        string body = await ReadBodyWithLimitAsync();
        return ParseFormPairs(body);
    }

    protected string? GetCookie(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        Cookie? cookie = Context.Request.Cookies[name];
        return cookie?.Value;
    }

    protected void SetCookie(string name, string value, bool httpOnly = true, int maxAgeSeconds = 86400)
    {
        string safeValue = value ?? string.Empty;
        string header = $"{name}={safeValue}; Path=/; Max-Age={maxAgeSeconds}; SameSite=Lax";
        if (httpOnly)
        {
            header += "; HttpOnly";
        }

        Context.Response.Headers.Add("Set-Cookie", header);
    }

    protected void DeleteCookie(string name)
    {
        string header = $"{name}=; Path=/; Max-Age=0; Expires=Thu, 01 Jan 1970 00:00:00 GMT; HttpOnly; SameSite=Lax";
        Context.Response.Headers.Add("Set-Cookie", header);
    }

    protected IActionResult Html(string html, int statusCode = 200)
    {
        return new HtmlResult(html, statusCode);
    }

    protected IActionResult Json(object? data, int statusCode = 200)
    {
        return new JsonResult(data, statusCode);
    }

    protected IActionResult Redirect(string location, int statusCode = 302)
    {
        return new RedirectResult(location, statusCode);
    }

    protected IActionResult NotFound(string message = "Запрошенный ресурс не найден.")
    {
        return new StatusResult(404, message);
    }

    protected IActionResult BadRequest(string message = "Некорректные данные запроса.")
    {
        return new StatusResult(400, message);
    }

    protected IActionResult Forbidden(string message = "У вас недостаточно прав для выполнения этого действия.")
    {
        return new StatusResult(403, message);
    }

    private async Task<string> ReadBodyWithLimitAsync()
    {
        HttpListenerRequest request = Context.Request;

        // Быстрая проверка заголовка Content-Length до чтения потока
        if (request.ContentLength64 > MaxBodyBytes)
        {
            throw new RequestTooLargeException(MaxBodyBytes);
        }

        using MemoryStream memory = new();
        byte[] buffer = new byte[8192];
        int totalRead = 0;

        while (true)
        {
            int bytesRead = await request.InputStream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                break;
            }

            totalRead += bytesRead;
            if (totalRead > MaxBodyBytes)
            {
                throw new RequestTooLargeException(MaxBodyBytes);
            }

            memory.Write(buffer, 0, bytesRead);
        }

        return Encoding.UTF8.GetString(memory.ToArray());
    }

    private static Dictionary<string, string> ParseQueryString(string? rawQuery)
    {
        if (string.IsNullOrEmpty(rawQuery))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        string trimmed = rawQuery.StartsWith('?') ? rawQuery.Substring(1) : rawQuery;
        return ParseFormPairs(trimmed);
    }

    private static Dictionary<string, string> ParseFormPairs(string rawData)
    {
        Dictionary<string, string> result = new(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(rawData))
        {
            return result;
        }

        string[] pairs = rawData.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (string pair in pairs)
        {
            int separatorIndex = pair.IndexOf('=');
            string rawKey = separatorIndex >= 0 ? pair.Substring(0, separatorIndex) : pair;
            string rawValue = separatorIndex >= 0 ? pair.Substring(separatorIndex + 1) : string.Empty;

            string key = DecodeSafely(rawKey);
            string value = DecodeSafely(rawValue);

            if (!string.IsNullOrEmpty(key))
            {
                result[key] = value;
            }
        }

        return result;
    }

    // Безопасное декодирование URL-значений (защита от битых %ZZ и нулевых байтов)
    private static string DecodeSafely(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        try
        {
            string decoded = WebUtility.UrlDecode(value) ?? string.Empty;
            return decoded.Replace("\0", string.Empty);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}
