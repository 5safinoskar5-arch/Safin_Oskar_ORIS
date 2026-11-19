using System.Text.Json;

public class ServerSettings
{
    public string Url { get; set; } = "http://localhost:8888/";
    public string RootDirectory { get; set; } = "static";
    public string DefaultDocument { get; set; } = "search-engine.html";

    public static ServerSettings Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "settings.json");
        string json = File.ReadAllText(path);

        ServerSettings settings = JsonSerializer.Deserialize<ServerSettings>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("settings.json пуст.");

        if (!Uri.TryCreate(settings.Url, UriKind.Absolute, out Uri? url)
            || url.Scheme != "http"
            || !url.IsLoopback
            || url.AbsolutePath != "/"
            || url.Query != ""
            || url.Fragment != ""
            || url.UserInfo != ""
            || !settings.Url.EndsWith('/'))
        {
            throw new InvalidOperationException("Url должен иметь вид http://localhost:8888/ или http://127.0.0.1:8888/.");
        }

        if (string.IsNullOrWhiteSpace(settings.RootDirectory))
            throw new InvalidOperationException("Не указан RootDirectory.");

        if (string.IsNullOrWhiteSpace(settings.DefaultDocument)
            || settings.DefaultDocument.Contains('/')
            || settings.DefaultDocument.Contains('\\')
            || settings.DefaultDocument.Contains(':')
            || settings.DefaultDocument is "." or "..")
        {
            throw new InvalidOperationException("DefaultDocument должен содержать имя HTML-файла без папок.");
        }

        return settings;
    }
}
