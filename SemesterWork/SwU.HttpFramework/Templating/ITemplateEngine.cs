namespace SwU.HttpFramework.Templating;

public interface ITemplateEngine
{
    string Render(string template, object? model);
    string RenderFile(string viewName, object? model, string? layoutName = "layout.html");
}
