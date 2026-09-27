using System.Text.Json;

string settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
string htmlPath = Path.Combine(AppContext.BaseDirectory, "search-engine.html");

string json = await File.ReadAllTextAsync(settingsPath);
ServerSettings settings = JsonSerializer.Deserialize<ServerSettings>(json)
    ?? throw new InvalidOperationException("Не удалось прочитать settings.json.");

if (string.IsNullOrWhiteSpace(settings.Url))
    throw new InvalidOperationException("В settings.json не указан Url.");

var server = new HttpServer(settings.Url, htmlPath);
Task serverTask = server.Start();

Console.WriteLine($"Открой {settings.Url}");
Console.WriteLine("Для остановки введи stop.");

while (Console.ReadLine() is string command)
{
    if (command.Trim().Equals("stop", StringComparison.OrdinalIgnoreCase))
        break;
}

server.Stop();
await serverTask;

public class ServerSettings
{
    public string Url { get; set; } = "";
}
