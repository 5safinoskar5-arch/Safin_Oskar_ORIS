using SwU.HttpFramework.Mvc;
using SwU.HttpFramework.Mvc.Results;
using SwU.HttpFramework.Templating;
using SwU.WebApp.Models;
using SwU.WebApp.Services;

namespace SwU.WebApp.Controllers;

public abstract class AppController : BaseController
{
    protected ITemplateEngine TemplateEngine { get; }

    protected AuthService AuthService { get; }

    protected AppController(ITemplateEngine templateEngine, AuthService authService)
    {
        TemplateEngine = templateEngine ?? throw new ArgumentNullException(nameof(templateEngine));
        AuthService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    protected Task<User?> GetCurrentUserAsync()
    {
        return AuthService.GetCurrentUserAsync(Context);
    }

    protected IActionResult RenderView(
        string viewName,
        Dictionary<string, object?> model,
        User? user,
        int statusCode = 200)
    {
        model["CurrentUser"] = user;
        model["IsAuthenticated"] = user != null;
        model["IsAnonymous"] = user == null;
        model["IsAdmin"] = user != null && user.Role == "Admin";
        model["DisplayName"] = user?.Username ?? string.Empty;
        model["CurrentYear"] = DateTime.Now.Year;

        if (!model.TryGetValue("PageTitle", out object? title) || string.IsNullOrWhiteSpace(title?.ToString()))
        {
            model["PageTitle"] = "Star Wars Unlimited DB";
        }

        string html = TemplateEngine.RenderFile(viewName, model);
        return Html(html, statusCode);
    }

    protected static string FormValue(Dictionary<string, string> form, string key)
    {
        return form.TryGetValue(key, out string? value) ? value ?? string.Empty : string.Empty;
    }

    protected static bool TryGetPositiveInt(Dictionary<string, string> form, string key, out int value)
    {
        value = 0;
        if (!int.TryParse(FormValue(form, key).Trim(), out int parsed))
        {
            return false;
        }

        if (parsed <= 0)
        {
            return false;
        }

        value = parsed;
        return true;
    }

    // Собирает фильтры каталога из query string: некорректные числа просто игнорируются
    protected CardFilter BuildCardFilter()
    {
        return new CardFilter
        {
            Search = GetQuery("search"),
            Aspect = GetQuery("aspect"),
            CardType = GetQuery("type"),
            Rarity = GetQuery("rarity"),
            SetId = GetQueryIntInRange("setId", 1, int.MaxValue),
            Trait = GetQuery("trait"),
            MinCost = GetQueryIntInRange("minCost", 0, 20),
            MaxCost = GetQueryIntInRange("maxCost", 0, 20),
            MinPower = GetQueryIntInRange("minPower", 0, 30),
            MaxPower = GetQueryIntInRange("maxPower", 0, 30),
            MinHp = GetQueryIntInRange("minHp", 0, 50),
            MaxHp = GetQueryIntInRange("maxHp", 0, 50),
            Sort = GetQuery("sort") ?? "default",
            Page = GetQueryIntInRange("page", 1, 100000) ?? 1
        };
    }

    // int.TryParse + проверка диапазона: значения вне диапазона считаем отсутствующими
    protected int? GetQueryIntInRange(string key, int min, int max)
    {
        int? value = GetQueryInt(key);
        if (!value.HasValue || value.Value < min || value.Value > max)
        {
            return null;
        }

        return value.Value;
    }

    protected static List<FilterOption> BuildSetOptions(
        List<CardSet> sets,
        int? selectedSetId,
        bool includeAll)
    {
        List<FilterOption> options = new();
        if (includeAll)
        {
            options.Add(new FilterOption(string.Empty, "All sets", !selectedSetId.HasValue));
        }

        foreach (CardSet set in sets)
        {
            bool isSelected = selectedSetId.HasValue && selectedSetId.Value == set.Id;
            options.Add(new FilterOption(
                set.Id.ToString(),
                set.Code + " — " + set.Name,
                isSelected));
        }

        return options;
    }
}
