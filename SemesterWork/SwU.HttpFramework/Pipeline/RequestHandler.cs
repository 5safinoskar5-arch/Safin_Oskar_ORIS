using System.Net;

namespace SwU.HttpFramework.Pipeline;

public abstract class RequestHandler
{
    public RequestHandler? Next { get; private set; }

    public RequestHandler SetNext(RequestHandler next)
    {
        Next = next ?? throw new ArgumentNullException(nameof(next));
        return next;
    }

    public abstract Task HandleAsync(HttpListenerContext context);
}
