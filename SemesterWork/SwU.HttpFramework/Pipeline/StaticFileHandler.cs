using System.Net;

namespace SwU.HttpFramework.Pipeline;

public class StaticFileHandler : RequestHandler
{
    private static readonly Dictionary<string, string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".html"] = "text/html; charset=utf-8",
        [".css"] = "text/css; charset=utf-8",
        [".js"] = "application/javascript; charset=utf-8",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".webp"] = "image/webp",
        [".svg"] = "image/svg+xml",
        [".ico"] = "image/x-icon",
        [".woff2"] = "font/woff2"
    };

    private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

    private readonly string _rootFullPath;

    public StaticFileHandler(string staticRootPath)
    {
        if (string.IsNullOrWhiteSpace(staticRootPath))
        {
            throw new ArgumentException("Путь к статическим файлам не может быть пустым.", nameof(staticRootPath));
        }

        _rootFullPath = Path.GetFullPath(staticRootPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    public override async Task HandleAsync(HttpListenerContext context)
    {
        string method = context.Request.HttpMethod;
        bool isGet = string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase);
        bool isHead = string.Equals(method, "HEAD", StringComparison.OrdinalIgnoreCase);

        if (!isGet && !isHead)
        {
            await PassToNextAsync(context);
            return;
        }

        // Проверяем исходный RawUrl до нормализации Uri, чтобы не пропустить encoded ".."
        string? rawUrlPath = ExtractPathWithoutQuery(context.Request.RawUrl);
        if (rawUrlPath != null && ContainsTraversalSequence(rawUrlPath))
        {
            await PassToNextAsync(context);
            return;
        }

        string? rawTarget = context.Request.Url?.AbsolutePath;
        if (!string.IsNullOrEmpty(rawTarget))
        {
            string? resolved = TryResolveStaticFilePath(rawTarget);
            if (resolved != null && File.Exists(resolved))
            {
                string extension = Path.GetExtension(resolved);
                if (AllowedMimeTypes.TryGetValue(extension, out string? contentType))
                {
                    await SendFileAsync(context, resolved, contentType, isHead);
                    return;
                }
            }
        }

        await PassToNextAsync(context);
    }

    private string? TryResolveStaticFilePath(string? rawPath)
    {
        if (string.IsNullOrEmpty(rawPath))
        {
            return null;
        }

        string decoded;
        try
        {
            decoded = Uri.UnescapeDataString(rawPath);
        }
        catch (Exception)
        {
            return null;
        }

        string normalized = decoded.Replace('\\', '/').TrimStart('/');
        if (!IsSafeRelativePath(normalized))
        {
            return null;
        }

        try
        {
            string fullPath = Path.GetFullPath(normalized, _rootFullPath);
            string expectedPrefix = _rootFullPath + Path.DirectorySeparatorChar;

            // Защита от path traversal: файл обязан находиться строго внутри каталога статики
            if (!fullPath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return fullPath;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool IsSafeRelativePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return false;
        }

        // Блокируем попытки выхода из каталога, нулевые байты и диски/NTFS-потоки
        if (relativePath.Contains("..", StringComparison.Ordinal) ||
            relativePath.Contains('\0') ||
            relativePath.Contains(':'))
        {
            return false;
        }

        if (relativePath.IndexOfAny(InvalidPathChars) >= 0)
        {
            return false;
        }

        return true;
    }

    private static string? ExtractPathWithoutQuery(string? rawUrl)
    {
        if (string.IsNullOrEmpty(rawUrl))
        {
            return null;
        }

        int queryIndex = rawUrl.IndexOf('?');
        return queryIndex >= 0 ? rawUrl.Substring(0, queryIndex) : rawUrl;
    }

    private static bool ContainsTraversalSequence(string rawPath)
    {
        if (rawPath.Contains("..", StringComparison.Ordinal) ||
            rawPath.Contains('\\') ||
            rawPath.Contains("%2e", StringComparison.OrdinalIgnoreCase) ||
            rawPath.Contains("%5c", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        try
        {
            string decoded = Uri.UnescapeDataString(rawPath);
            return decoded.Contains("..", StringComparison.Ordinal) ||
                decoded.Contains('\\') ||
                decoded.Contains('\0');
        }
        catch (Exception)
        {
            return true;
        }
    }

    private static async Task SendFileAsync(
        HttpListenerContext context,
        string filePath,
        string contentType,
        bool isHead)
    {
        try
        {
            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
            HttpListenerResponse response = context.Response;

            response.StatusCode = 200;
            response.ContentType = contentType;
            response.ContentLength64 = fileBytes.Length;

            if (!isHead)
            {
                await response.OutputStream.WriteAsync(fileBytes, 0, fileBytes.Length);
            }

            response.Close();
        }
        catch (HttpListenerException)
        {
            // Клиент оборвал соединение во время скачивания файла
        }
        catch (ObjectDisposedException)
        {
            // Ответ уже закрыт
        }
    }

    private async Task PassToNextAsync(HttpListenerContext context)
    {
        if (Next != null)
        {
            await Next.HandleAsync(context);
        }
    }
}
