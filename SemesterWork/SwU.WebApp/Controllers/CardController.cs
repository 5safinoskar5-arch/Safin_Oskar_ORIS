using Npgsql;
using SwU.HttpFramework.Mvc;
using SwU.HttpFramework.Mvc.Results;
using SwU.HttpFramework.Templating;
using SwU.WebApp.Models;
using SwU.WebApp.Repositories;
using SwU.WebApp.Services;

namespace SwU.WebApp.Controllers;

[Controller]
public class CardController : AppController
{
    private static readonly string[] AspectValues =
    {
        "Aggression", "Command", "Cunning", "Heroism", "Vigilance", "Villainy"
    };

    private static readonly string[] CardTypeValues =
    {
        "Leader", "Unit", "Upgrade", "Event", "Base", "Token"
    };

    private static readonly string[] RarityValues =
    {
        "Common", "Uncommon", "Rare", "Legendary", "Special", "Starter"
    };

    private static readonly KeyValuePair<string, string>[] SortValues =
    {
        new("default", "Default"),
        new("cost_asc", "Cost Asc"),
        new("cost_desc", "Cost Desc"),
        new("hp_desc", "Health Desc"),
        new("name", "Name")
    };

    private readonly CardRepository _cards;
    private readonly SetRepository _sets;
    private readonly CommentRepository _comments;
    private readonly ValidationService _validation;
    private readonly HtmlSanitizer _sanitizer;

    public CardController(
        CardRepository cards,
        SetRepository sets,
        CommentRepository comments,
        AuthService authService,
        ITemplateEngine templateEngine,
        ValidationService validation,
        HtmlSanitizer sanitizer)
        : base(templateEngine, authService)
    {
        _cards = cards;
        _sets = sets;
        _comments = comments;
        _validation = validation;
        _sanitizer = sanitizer;
    }

    [HttpGet("/")]
    [HttpGet("/cards")]
    public async Task<IActionResult> Index()
    {
        User? user = await GetCurrentUserAsync();

        CardFilter filter = BuildCardFilter();
        CardPage page = await _cards.SearchAsync(filter);
        List<CardSet> sets = await _sets.GetAllOrderedAsync();

        Dictionary<string, object?> model = new()
        {
            ["PageTitle"] = "All cards",
            ["Cards"] = page.Items,
            ["CardCount"] = page.TotalCount,
            ["Search"] = filter.Search ?? string.Empty,
            ["SelectedAspect"] = filter.Aspect ?? string.Empty,
            ["AspectOptions"] = BuildOptions(AspectValues, filter.Aspect, "All aspects"),
            ["TypeOptions"] = BuildOptions(CardTypeValues, filter.CardType, "All types"),
            ["RarityOptions"] = BuildOptions(RarityValues, filter.Rarity, "All rarities"),
            ["SetOptions"] = BuildSetOptions(sets, filter.SetId, includeAll: true),
            ["SortOptions"] = BuildSortOptions(filter.Sort),
            ["TraitValue"] = filter.Trait ?? string.Empty,
            ["MinCost"] = ToText(filter.MinCost),
            ["MaxCost"] = ToText(filter.MaxCost),
            ["MinPower"] = ToText(filter.MinPower),
            ["MaxPower"] = ToText(filter.MaxPower),
            ["MinHp"] = ToText(filter.MinHp),
            ["MaxHp"] = ToText(filter.MaxHp),
            ["PageLinks"] = BuildPageLinks(filter, page)
        };

        return RenderView("cards.html", model, user);
    }

    [HttpGet("/cards/detail")]
    public async Task<IActionResult> Detail()
    {
        int? id = GetQueryInt("id");
        if (!id.HasValue || id.Value <= 0)
        {
            return NotFound("Card not found.");
        }

        User? user = await GetCurrentUserAsync();
        return await RenderDetailAsync(id.Value, user, null, 200);
    }

    [HttpPost("/cards/comment")]
    public async Task<IActionResult> AddComment()
    {
        User? user = await GetCurrentUserAsync();
        if (user == null)
        {
            return Redirect("/login?error=login_required");
        }

        Dictionary<string, string> form = await ReadFormAsync();
        if (!TryGetPositiveInt(form, "cardId", out int cardId))
        {
            return BadRequest("Invalid card id.");
        }

        Card? card = await _cards.GetByIdAsync(cardId);
        if (card == null)
        {
            return NotFound("Card not found.");
        }

        if (!TryValidateCommentForm(form, out string title, out string body, out int rating, out string error))
        {
            return await RenderDetailAsync(cardId, user, error, 400);
        }

        CardComment comment = new()
        {
            CardId = cardId,
            UserId = user.Id,
            Title = title,
            Body = body,
            Rating = rating,
            CreatedAt = DateTime.Now
        };

        try
        {
            await _comments.CreateAsync(comment);
        }
        catch (PostgresException exception)
        {
            return await RenderDetailAsync(cardId, user, DbErrorHelper.GetMessage(exception), 400);
        }

        return Redirect($"/cards/detail?id={cardId}&comment=added");
    }

    [HttpGet("/sets")]
    public IActionResult RedirectToSets()
    {
        return Redirect("https://sw-unlimited-db.com/sets/");
    }

    [HttpGet("/decks")]
    public IActionResult RedirectToDecks()
    {
        return Redirect("https://sw-unlimited-db.com/decks/");
    }

    [HttpGet("/meta")]
    public IActionResult RedirectToMeta()
    {
        return Redirect("https://sw-unlimited-db.com/meta/");
    }

    [HttpGet("/collection")]
    public IActionResult RedirectToCollection()
    {
        return Redirect("https://sw-unlimited-db.com/collection/");
    }

    [HttpGet("/community")]
    public IActionResult RedirectToCommunity()
    {
        return Redirect("https://sw-unlimited-db.com/community/");
    }

    [HttpGet("/proxy-cards")]
    public IActionResult RedirectToProxyCards()
    {
        return Redirect("https://sw-unlimited-db.com/proxy-cards/");
    }

    private async Task<IActionResult> RenderDetailAsync(
        int cardId,
        User? user,
        string? errorMessage,
        int statusCode)
    {
        Card? card = await _cards.GetByIdAsync(cardId);
        if (card == null)
        {
            return NotFound("Card not found.");
        }

        CardSet? set = await _sets.GetByIdAsync(card.SetId);
        List<CommentViewItem> comments = await _comments.GetByCardIdAsync(cardId);
        bool commentAdded = string.Equals(GetQuery("comment"), "added", StringComparison.OrdinalIgnoreCase);

        Dictionary<string, object?> model = new()
        {
            ["PageTitle"] = card.Name,
            ["Card"] = card,
            ["SetName"] = set?.Name ?? "Unknown set",
            ["SetCode"] = set?.Code ?? "—",
            ["Comments"] = comments,
            ["CommentCount"] = comments.Count,
            ["CommentAdded"] = commentAdded,
            ["CommentError"] = errorMessage ?? string.Empty,
            ["AbilityHtml"] = _sanitizer.Sanitize(card.AbilityText),
            ["HasArtist"] = !string.IsNullOrWhiteSpace(card.Artist),
            ["Aspects"] = SplitCommaSeparated(card.Aspect),
            ["Traits"] = SplitCommaSeparated(card.Traits)
        };

        return RenderView("card-detail.html", model, user, statusCode);
    }

    // Окно из трех номеров страниц плюс Previous/Next, как в оригинальном каталоге
    private static List<PageLink> BuildPageLinks(CardFilter filter, CardPage page)
    {
        List<PageLink> links = new();
        if (page.TotalPages <= 1)
        {
            return links;
        }

        links.Add(page.Page > 1
            ? PageLink.Create("Previous", page.Page - 1, BuildPageUrl(filter, page.Page - 1))
            : PageLink.Disabled("Previous"));

        int start = Math.Max(1, page.Page - 1);
        int end = Math.Min(page.TotalPages, start + 2);
        start = Math.Max(1, end - 2);

        for (int number = start; number <= end; number++)
        {
            links.Add(number == page.Page
                ? PageLink.Active(number.ToString())
                : PageLink.Create(number.ToString(), number, BuildPageUrl(filter, number)));
        }

        links.Add(page.Page < page.TotalPages
            ? PageLink.Create("Next", page.Page + 1, BuildPageUrl(filter, page.Page + 1))
            : PageLink.Disabled("Next"));

        return links;
    }

    // Ссылка пагинации сохраняет все активные фильтры, чтобы страницы не терялись
    private static string BuildPageUrl(CardFilter filter, int pageNumber)
    {
        List<string> parts = new();
        AddQueryPart(parts, "search", filter.Search);
        AddQueryPart(parts, "aspect", filter.Aspect);
        AddQueryPart(parts, "type", filter.CardType);
        AddQueryPart(parts, "rarity", filter.Rarity);
        AddQueryPart(parts, "setId", ToText(filter.SetId));
        AddQueryPart(parts, "trait", filter.Trait);
        AddQueryPart(parts, "minCost", ToText(filter.MinCost));
        AddQueryPart(parts, "maxCost", ToText(filter.MaxCost));
        AddQueryPart(parts, "minPower", ToText(filter.MinPower));
        AddQueryPart(parts, "maxPower", ToText(filter.MaxPower));
        AddQueryPart(parts, "minHp", ToText(filter.MinHp));
        AddQueryPart(parts, "maxHp", ToText(filter.MaxHp));
        AddQueryPart(parts, "sort", filter.Sort);
        AddQueryPart(parts, "page", pageNumber.ToString());

        return "/cards?" + string.Join("&", parts);
    }

    private static void AddQueryPart(List<string> parts, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        parts.Add(key + "=" + Uri.EscapeDataString(value.Trim()));
    }

    private static string ToText(int? value)
    {
        return value.HasValue ? value.Value.ToString() : string.Empty;
    }

    private static List<FilterOption> BuildOptions(string[] values, string? selected, string allText)
    {
        List<FilterOption> options = new()
        {
            new FilterOption(string.Empty, allText, string.IsNullOrWhiteSpace(selected))
        };

        foreach (string value in values)
        {
            bool isSelected = string.Equals(value, selected, StringComparison.OrdinalIgnoreCase);
            options.Add(new FilterOption(value, value, isSelected));
        }

        return options;
    }

    private static List<FilterOption> BuildSortOptions(string? selected)
    {
        List<FilterOption> options = new();
        foreach (KeyValuePair<string, string> pair in SortValues)
        {
            bool isSelected = string.Equals(pair.Key, selected, StringComparison.OrdinalIgnoreCase);
            options.Add(new FilterOption(pair.Key, pair.Value, isSelected));
        }

        return options;
    }

    private bool TryValidateCommentForm(
        Dictionary<string, string> form,
        out string title,
        out string body,
        out int rating,
        out string error)
    {
        body = FormValue(form, "body").Trim();
        title = FormValue(form, "title").Trim();
        if (title.Length == 0)
        {
            title = BuildAutoTitle(body);
        }

        rating = 0;
        if (!_validation.IsValidComment(body))
        {
            error = "Comment must be between 3 and 500 characters.";
            return false;
        }

        if (!_validation.IsValidCommentTitle(title))
        {
            error = "Title must be between 3 and 120 characters.";
            return false;
        }

        if (!int.TryParse(FormValue(form, "rating").Trim(), out rating) ||
            !_validation.IsValidRating(rating))
        {
            error = "Rating must be a whole number from 1 to 5.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static List<string> SplitCommaSeparated(string? value)
    {
        List<string> result = new();
        if (string.IsNullOrWhiteSpace(value))
        {
            return result;
        }

        string[] parts = value.Split(',');
        foreach (string part in parts)
        {
            string trimmed = part.Trim();
            if (trimmed.Length > 0)
            {
                result.Add(trimmed);
            }
        }

        return result;
    }

    private static string BuildAutoTitle(string body)
    {
        string singleLine = body.Replace('\r', ' ').Replace('\n', ' ').Trim();
        return singleLine.Length <= 80 ? singleLine : singleLine.Substring(0, 80);
    }
}
