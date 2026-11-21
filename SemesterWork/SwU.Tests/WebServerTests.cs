using System.Net;
using System.Net.Sockets;
using System.Text;
using SwU.HttpFramework.Mvc;
using SwU.HttpFramework.Mvc.Results;
using SwU.HttpFramework.Pipeline;
using SwU.HttpFramework.Server;

namespace SwU.Tests;

[TestClass]
public class WebServerTests
{
    private WebServer? _server;
    private CancellationTokenSource? _shutdown;
    private HttpClient? _client;
    private Task? _serverTask;
    private string _staticRoot = string.Empty;
    private int _port;

    [TestInitialize]
    public void StartServer()
    {
        _staticRoot = Path.Combine(Path.GetTempPath(), "swu-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_staticRoot);

        _port = FindFreePort();
        ServerConfig config = new()
        {
            Prefix = $"http://127.0.0.1:{_port}/",
            StaticRootPath = _staticRoot,
            ViewsRootPath = _staticRoot,
            MaxBodyBytes = 262144
        };

        RouterHandler router = new(config.MaxBodyBytes);
        router.RegisterController(() => new TestController());

        StaticFileHandler staticFileHandler = new(config.StaticRootPath);
        staticFileHandler.SetNext(router).SetNext(new NotFoundHandler());

        _server = new WebServer(config, staticFileHandler);
        _shutdown = new CancellationTokenSource();
        _serverTask = _server.StartAsync(_shutdown.Token);

        _client = new HttpClient
        {
            BaseAddress = new Uri($"http://127.0.0.1:{_port}/"),
            Timeout = TimeSpan.FromSeconds(5)
        };

        WaitUntilReady();
    }

    [TestCleanup]
    public void StopServer()
    {
        _client?.Dispose();

        if (_shutdown != null)
        {
            _shutdown.Cancel();
        }

        if (_serverTask != null)
        {
            try
            {
                _serverTask.Wait(TimeSpan.FromSeconds(5));
            }
            catch (AggregateException)
            {
                // Останавливаемый сервер мог завершиться с ожидаемым исключением
            }
        }

        _shutdown?.Dispose();

        try
        {
            Directory.Delete(_staticRoot, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    [TestMethod]
    public void Get_ExistingRoute_Returns200()
    {
        using HttpResponseMessage response = _client!.GetAsync("test").GetAwaiter().GetResult();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public void Get_UnknownRoute_Returns404()
    {
        using HttpResponseMessage response = _client!.GetAsync("does-not-exist").GetAwaiter().GetResult();

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public void Get_OnPostOnlyRoute_Returns405()
    {
        using HttpResponseMessage response = _client!.GetAsync("submit").GetAwaiter().GetResult();

        Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [TestMethod]
    public void ThrowingRoute_Returns500_AndServerKeepsWorking()
    {
        using HttpResponseMessage failed = _client!.GetAsync("boom").GetAwaiter().GetResult();
        Assert.AreEqual(HttpStatusCode.InternalServerError, failed.StatusCode);

        using HttpResponseMessage healthy = _client.GetAsync("test").GetAwaiter().GetResult();
        Assert.AreEqual(HttpStatusCode.OK, healthy.StatusCode);
    }

    [TestMethod]
    [DataRow("/../../etc/passwd")]
    [DataRow("/..%5c..%5cProgram.cs")]
    public void PathTraversal_IsNeverServedWith200(string rawTarget)
    {
        string rawResponse = SendRawGet(rawTarget);

        Assert.IsFalse(
            rawResponse.StartsWith("HTTP/1.1 200", StringComparison.Ordinal),
            "Path traversal не должен возвращать 200: " + rawTarget);
    }

    [TestMethod]
    public void JsonResult_UsesCamelCasePropertyNames()
    {
        using HttpResponseMessage response = _client!.GetAsync("json").GetAwaiter().GetResult();
        string body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(body, "\"name\":\"Luke\"");
        StringAssert.Contains(body, "\"cardNumber\":\"SOR-001\"");
        StringAssert.Contains(body, "\"setCode\":\"SOR\"");
        Assert.IsFalse(body.Contains("\"Name\"", StringComparison.Ordinal));
        Assert.IsFalse(body.Contains("\"CardNumber\"", StringComparison.Ordinal));
    }

    private void WaitUntilReady()
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            try
            {
                using HttpResponseMessage response = _client!.GetAsync("test").GetAwaiter().GetResult();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // Сервер ещё не начал принимать соединения
            }
            catch (TaskCanceledException)
            {
                // Запрос не успел выполниться до запуска сервера
            }

            Thread.Sleep(25);
        }

        Assert.Fail("Тестовый HTTP-сервер не запустился за отведённое время.");
    }

    private string SendRawGet(string rawTarget)
    {
        using TcpClient tcpClient = new();
        tcpClient.Connect(IPAddress.Loopback, _port);
        tcpClient.ReceiveTimeout = 5000;

        using NetworkStream stream = tcpClient.GetStream();
        stream.ReadTimeout = 5000;

        string request =
            "GET " + rawTarget + " HTTP/1.1\r\n" +
            "Host: 127.0.0.1:" + _port + "\r\n" +
            "Connection: close\r\n\r\n";

        byte[] requestBytes = Encoding.ASCII.GetBytes(request);
        stream.Write(requestBytes, 0, requestBytes.Length);

        using StreamReader reader = new(stream, Encoding.ASCII);
        return reader.ReadToEnd();
    }

    private static int FindFreePort()
    {
        TcpListener listener = new(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    // Тестовый контроллер с маршрутами для проверки HTTP-пайплайна
    private sealed class TestController : BaseController
    {
        [HttpGet("/test")]
        public IActionResult Test()
        {
            return Html("<h1>OK</h1>");
        }

        [HttpPost("/submit")]
        public IActionResult Submit()
        {
            return Html("posted");
        }

        [HttpGet("/boom")]
        public IActionResult Boom()
        {
            throw new InvalidOperationException("Тестовое исключение");
        }

        [HttpGet("/json")]
        public IActionResult JsonData()
        {
            return Json(new
            {
                Name = "Luke",
                CardNumber = "SOR-001",
                SetCode = "SOR"
            });
        }
    }
}
