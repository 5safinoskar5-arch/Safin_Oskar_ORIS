using System.Text;

Console.OutputEncoding = Encoding.UTF8;
HttpServer? server = null;

try
{
    ServerSettings settings = ServerSettings.Load();
    server = new HttpServer(settings);
    server.Start();

    Console.WriteLine($"Открой {settings.Url}");
    Console.WriteLine("Для остановки введи stop.");

    while (Console.ReadLine() is string command)
    {
        if (command.Trim().Equals("stop", StringComparison.OrdinalIgnoreCase))
            break;
    }
}
catch (Exception exception)
{
    Console.WriteLine($"Ошибка запуска: {exception.Message}");
}
finally
{
    if (server != null)
        await server.Stop();
}
