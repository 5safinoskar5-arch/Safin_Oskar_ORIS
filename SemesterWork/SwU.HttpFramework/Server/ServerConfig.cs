namespace SwU.HttpFramework.Server;

public class ServerConfig
{
    public string Prefix { get; set; } = "http://localhost:8080/";
    public string StaticRootPath { get; set; } = "wwwroot";
    public string ViewsRootPath { get; set; } = "Views";
    public int MaxBodyBytes { get; set; } = 262144;
}
