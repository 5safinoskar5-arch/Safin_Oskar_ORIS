using System.Net;
using System.Reflection;
using System.Text;
using SwU.HttpFramework.Mvc;
using SwU.HttpFramework.Mvc.Results;
using SwU.HttpFramework.Server;

namespace SwU.HttpFramework.Pipeline;

public class RouterHandler : RequestHandler
{
    private readonly List<RouteEntry> _routes = new();
    private readonly int _maxBodyBytes;

    public RouterHandler(int maxBodyBytes = 262144)
    {
        _maxBodyBytes = maxBodyBytes;
    }

    public void RegisterController<TController>(Func<TController> factory)
        where TController : BaseController
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        Type controllerType = typeof(TController);
        MethodInfo[] methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        foreach (MethodInfo method in methods)
        {
            IEnumerable<HttpMethodAttribute> attributes = method.GetCustomAttributes<HttpMethodAttribute>();
            foreach (HttpMethodAttribute attribute in attributes)
            {
                ValidateActionSignature(method);
                string normalizedPath = NormalizePath(attribute.Route);
                _routes.Add(new RouteEntry(attribute.HttpMethod, normalizedPath, method, () => factory()));
            }
        }
    }

    public override async Task HandleAsync(HttpListenerContext context)
    {
        string? normalizedPath = TryExtractNormalizedPath(context.Request.Url?.AbsolutePath);
        if (normalizedPath == null)
        {
            IActionResult badRequest = new StatusResult(400, "Некорректный формат URL-адреса.");
            await badRequest.ExecuteAsync(context);
            return;
        }

        string httpMethod = context.Request.HttpMethod;
        RouteEntry? matchedRoute = FindRoute(normalizedPath, httpMethod, out bool pathExists);

        if (matchedRoute != null)
        {
            await InvokeActionAsync(context, matchedRoute);
            return;
        }

        if (pathExists)
        {
            string html = ErrorPageBuilder.Build(
                405,
                "Method not allowed",
                $"HTTP method {httpMethod} is not supported for {normalizedPath}.");
            await ErrorPageBuilder.WriteHtmlSafelyAsync(context, 405, html);
            return;
        }

        if (Next != null)
        {
            await Next.HandleAsync(context);
        }
    }

    private RouteEntry? FindRoute(string normalizedPath, string httpMethod, out bool pathExists)
    {
        pathExists = false;

        foreach (RouteEntry route in _routes)
        {
            if (!string.Equals(route.Path, normalizedPath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            pathExists = true;
            if (string.Equals(route.HttpMethod, httpMethod, StringComparison.OrdinalIgnoreCase))
            {
                return route;
            }
        }

        return null;
    }

    private async Task InvokeActionAsync(HttpListenerContext context, RouteEntry route)
    {
        BaseController controller = route.Factory();
        controller.Context = context;
        controller.MaxBodyBytes = _maxBodyBytes;

        try
        {
            object? rawResult = route.Method.Invoke(controller, null);
            IActionResult? actionResult = await UnwrapActionResultAsync(rawResult);
            if (actionResult != null)
            {
                await actionResult.ExecuteAsync(context);
            }
        }
        catch (TargetInvocationException ex) when (ex.InnerException is RequestTooLargeException tooLarge)
        {
            IActionResult badRequest = new StatusResult(400, tooLarge.Message);
            await badRequest.ExecuteAsync(context);
        }
        catch (RequestTooLargeException tooLarge)
        {
            IActionResult badRequest = new StatusResult(400, tooLarge.Message);
            await badRequest.ExecuteAsync(context);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException;
        }
    }

    private static async Task<IActionResult?> UnwrapActionResultAsync(object? rawResult)
    {
        if (rawResult is IActionResult directResult)
        {
            return directResult;
        }

        if (rawResult is Task<IActionResult> asyncResult)
        {
            return await asyncResult;
        }

        if (rawResult is Task plainTask)
        {
            await plainTask;
            return null;
        }

        return null;
    }

    private static void ValidateActionSignature(MethodInfo method)
    {
        if (method.GetParameters().Length != 0)
        {
            throw new InvalidOperationException(
                $"Метод действия {method.DeclaringType?.Name}.{method.Name} не должен принимать параметры.");
        }
    }

    private static string? TryExtractNormalizedPath(string? rawPath)
    {
        if (string.IsNullOrEmpty(rawPath))
        {
            return "/";
        }

        try
        {
            string decoded = Uri.UnescapeDataString(rawPath);
            if (decoded.Contains('\0'))
            {
                return null;
            }

            return NormalizePath(decoded);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        string replaced = path.Trim().Replace('\\', '/');
        StringBuilder builder = new();
        bool previousSlash = false;

        // Схлопываем подряд идущие слэши, например "//cards///" -> "/cards"
        foreach (char ch in replaced)
        {
            if (ch == '/')
            {
                if (!previousSlash)
                {
                    builder.Append('/');
                    previousSlash = true;
                }
            }
            else
            {
                builder.Append(ch);
                previousSlash = false;
            }
        }

        string normalized = builder.ToString();
        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized;
        }

        if (normalized.Length > 1 && normalized.EndsWith('/'))
        {
            normalized = normalized.TrimEnd('/');
        }

        return normalized.ToLowerInvariant();
    }

    private sealed class RouteEntry
    {
        public string HttpMethod { get; }
        public string Path { get; }
        public MethodInfo Method { get; }
        public Func<BaseController> Factory { get; }

        public RouteEntry(string httpMethod, string path, MethodInfo method, Func<BaseController> factory)
        {
            HttpMethod = httpMethod;
            Path = path;
            Method = method;
            Factory = factory;
        }
    }
}
