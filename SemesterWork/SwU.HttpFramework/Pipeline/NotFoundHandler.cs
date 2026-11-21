using System.Net;
using SwU.HttpFramework.Server;

namespace SwU.HttpFramework.Pipeline;

public class NotFoundHandler : RequestHandler
{
    public override async Task HandleAsync(HttpListenerContext context)
    {
        string html = ErrorPageBuilder.Build(
            404,
            "Page not found",
            "The requested page does not exist or has been moved.");
        await ErrorPageBuilder.WriteHtmlSafelyAsync(context, 404, html);
    }
}
