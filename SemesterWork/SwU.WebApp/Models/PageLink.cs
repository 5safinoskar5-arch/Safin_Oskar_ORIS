namespace SwU.WebApp.Models;

// Элемент панели пагинации: ссылка, активная страница или недоступная кнопка
public class PageLink
{
    public string Text { get; set; } = string.Empty;

    public int Number { get; set; }

    public string Url { get; set; } = string.Empty;

    public string CssClass { get; set; } = string.Empty;

    public static PageLink Create(string text, int number, string url)
    {
        return new PageLink { Text = text, Number = number, Url = url };
    }

    public static PageLink Active(string text)
    {
        return new PageLink { Text = text, CssClass = "is-active" };
    }

    public static PageLink Disabled(string text)
    {
        return new PageLink { Text = text, CssClass = "is-disabled" };
    }
}
