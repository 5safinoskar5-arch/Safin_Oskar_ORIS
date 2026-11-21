using System.Net;

namespace SwU.HttpFramework.Mvc.Results;

public interface IActionResult
{
    Task ExecuteAsync(HttpListenerContext context);
}
