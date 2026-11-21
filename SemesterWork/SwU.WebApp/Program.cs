using System.Net;
using System.Text;
using SwU.HttpFramework.Pipeline;
using SwU.HttpFramework.Server;
using SwU.HttpFramework.Templating;
using SwU.Orm;
using SwU.WebApp.Controllers;
using SwU.WebApp.Repositories;
using SwU.WebApp.Services;

Console.OutputEncoding = Encoding.UTF8;

string httpPrefix = Environment.GetEnvironmentVariable("HTTP_PREFIX") ?? "http://localhost:8080/";
string? connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("Не задана переменная окружения DB_CONNECTION_STRING.");
    Console.Error.WriteLine("Пример: Host=localhost;Port=5432;Database=swu;Username=postgres;Password=postgres");
    return 1;
}

string baseDirectory = AppContext.BaseDirectory;
string viewsPath = Path.Combine(baseDirectory, "Views");
string staticPath = Path.Combine(baseDirectory, "wwwroot");

if (!Directory.Exists(viewsPath) || !Directory.Exists(staticPath))
{
    Console.Error.WriteLine("Не найдены каталоги Views или wwwroot рядом с приложением.");
    Console.Error.WriteLine($"Путь приложения: {baseDirectory}");
    return 1;
}

ServerConfig config = new()
{
    Prefix = httpPrefix,
    ViewsRootPath = viewsPath,
    StaticRootPath = staticPath,
    MaxBodyBytes = 262144
};

// Ручная сборка зависимостей без DI-контейнера
PostgresDbContext dbContext = new(connectionString);

// В Docker PostgreSQL может перезапускаться после init-скриптов,
// поэтому даём базе несколько секунд на готовность, а не падаем сразу
if (!await WaitForDatabaseAsync(dbContext, attempts: 5, delayMs: 1000))
{
    Console.Error.WriteLine("PostgreSQL недоступен. Сервер продолжит работу: страницы с данными будут возвращать ошибку.");
}

CardRepository cardRepository = new(dbContext);
SetRepository setRepository = new(dbContext);
UserRepository userRepository = new(dbContext);
CommentRepository commentRepository = new(dbContext);

ValidationService validationService = new();
HtmlSanitizer htmlSanitizer = new();
AuthService authService = new(userRepository);
SimpleTemplateEngine templateEngine = new(viewsPath);

RouterHandler router = new(config.MaxBodyBytes);
router.RegisterController(() => new CardController(
    cardRepository,
    setRepository,
    commentRepository,
    authService,
    templateEngine,
    validationService,
    htmlSanitizer));
router.RegisterController(() => new AuthController(
    userRepository,
    authService,
    templateEngine,
    validationService));
router.RegisterController(() => new AdminController(
    cardRepository,
    setRepository,
    authService,
    templateEngine,
    validationService,
    htmlSanitizer));
router.RegisterController(() => new ApiController(
    cardRepository,
    authService,
    templateEngine));

// Цепочка обязанностей: статика -> маршруты -> 404
StaticFileHandler staticFileHandler = new(config.StaticRootPath);
NotFoundHandler notFoundHandler = new();
staticFileHandler.SetNext(router).SetNext(notFoundHandler);

WebServer server = new(config, staticFileHandler);
Console.WriteLine($"Сервер запускается: {config.Prefix}");

using CancellationTokenSource shutdown = new();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    shutdown.Cancel();
};

try
{
    await server.StartAsync(shutdown.Token);
}
catch (HttpListenerException exception)
{
    Console.Error.WriteLine($"Не удалось запустить HTTP-сервер: {exception.Message}");
    return 1;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Критическая ошибка запуска: {exception.Message}");
    return 1;
}

return 0;

// Несколько попыток подключения к БД с паузой между ними
static async Task<bool> WaitForDatabaseAsync(PostgresDbContext db, int attempts, int delayMs)
{
    for (int attempt = 1; attempt <= attempts; attempt++)
    {
        try
        {
            await db.ExecuteAsync("SELECT 1");
            Console.WriteLine($"Подключение к БД установлено (попытка {attempt}).");
            return true;
        }
        catch (Exception exception)
        {
            Console.WriteLine($"БД пока недоступна (попытка {attempt}/{attempts}): {exception.Message}");

            if (attempt < attempts)
            {
                await Task.Delay(delayMs);
            }
        }
    }

    return false;
}
