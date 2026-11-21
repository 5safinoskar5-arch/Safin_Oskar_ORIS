using System.Globalization;
using Npgsql;
using SwU.HttpFramework.Mvc;
using SwU.HttpFramework.Mvc.Results;
using SwU.HttpFramework.Templating;
using SwU.WebApp.Models;
using SwU.WebApp.Repositories;
using SwU.WebApp.Services;

namespace SwU.WebApp.Controllers;

[Controller]
public class AdminController : AppController
{
    private readonly CardRepository _cards;
    private readonly SetRepository _sets;
    private readonly ValidationService _validation;
    private readonly HtmlSanitizer _sanitizer;

    public AdminController(
        CardRepository cards,
        SetRepository sets,
        AuthService authService,
        ITemplateEngine templateEngine,
        ValidationService validation,
        HtmlSanitizer sanitizer)
        : base(templateEngine, authService)
    {
        _cards = cards;
        _sets = sets;
        _validation = validation;
        _sanitizer = sanitizer;
    }

    [HttpGet("/admin")]
    public async Task<IActionResult> Index()
    {
        User? user = await GetCurrentUserAsync();
        if (user == null || user.Role != "Admin")
        {
            return Redirect("/login?error=access_denied");
        }

        string notice = GetNoticeMessage(GetQuery("notice"));
        return await RenderAdminAsync(user, notice, null, null, 200);
    }

    [HttpGet("/admin/cards/create")]
    public async Task<IActionResult> CreateCardPage()
    {
        User? user = await GetCurrentUserAsync();
        if (user == null || user.Role != "Admin")
        {
            return Redirect("/login?error=access_denied");
        }

        Card card = new();
        return await RenderCardFormAsync(user, card, "create", null, 200);
    }

    [HttpPost("/admin/cards/create")]
    public async Task<IActionResult> CreateCard()
    {
        User? user = await GetCurrentUserAsync();
        if (user == null || user.Role != "Admin")
        {
            return Redirect("/login?error=access_denied");
        }

        Dictionary<string, string> form = await ReadFormAsync();
        if (!TryReadCardForm(form, out Card card, out string error))
        {
            return await RenderCardFormAsync(user, card, "create", error, 400);
        }

        if (!TryValidateCard(card, out error))
        {
            return await RenderCardFormAsync(user, card, "create", error, 400);
        }

        try
        {
            await _cards.CreateAsync(card);
        }
        catch (PostgresException exception)
        {
            string message = DbErrorHelper.GetMessage(exception, "Карта с таким номером уже существует.");
            return await RenderCardFormAsync(user, card, "create", message, 400);
        }

        return Redirect("/admin?notice=card_created");
    }

    [HttpGet("/admin/cards/edit")]
    public async Task<IActionResult> EditCardPage()
    {
        User? user = await GetCurrentUserAsync();
        if (user == null || user.Role != "Admin")
        {
            return Redirect("/login?error=access_denied");
        }

        int? id = GetQueryInt("id");
        if (!id.HasValue || id.Value <= 0)
        {
            return NotFound("Карта не найдена.");
        }

        Card? card = await _cards.GetByIdAsync(id.Value);
        if (card == null)
        {
            return NotFound("Карта не найдена.");
        }

        card.AbilityText = _sanitizer.Sanitize(card.AbilityText);
        return await RenderCardFormAsync(user, card, "edit", null, 200);
    }

    [HttpPost("/admin/cards/edit")]
    public async Task<IActionResult> EditCard()
    {
        User? user = await GetCurrentUserAsync();
        if (user == null || user.Role != "Admin")
        {
            return Redirect("/login?error=access_denied");
        }

        Dictionary<string, string> form = await ReadFormAsync();
        if (!TryGetPositiveInt(form, "id", out int id))
        {
            return BadRequest("Некорректный идентификатор карты.");
        }

        Card? existing = await _cards.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound("Карта не найдена.");
        }

        if (!TryReadCardForm(form, out Card card, out string error))
        {
            card.Id = id;
            return await RenderCardFormAsync(user, card, "edit", error, 400);
        }

        card.Id = id;
        if (!TryValidateCard(card, out error))
        {
            return await RenderCardFormAsync(user, card, "edit", error, 400);
        }

        try
        {
            await _cards.UpdateAsync(card);
        }
        catch (PostgresException exception)
        {
            string message = DbErrorHelper.GetMessage(exception, "Карта с таким номером уже существует.");
            return await RenderCardFormAsync(user, card, "edit", message, 400);
        }

        return Redirect("/admin?notice=card_updated");
    }

    [HttpPost("/admin/cards/delete")]
    public async Task<IActionResult> DeleteCard()
    {
        User? user = await GetCurrentUserAsync();
        if (user == null || user.Role != "Admin")
        {
            return Redirect("/login?error=access_denied");
        }

        Dictionary<string, string> form = await ReadFormAsync();
        if (!TryGetPositiveInt(form, "id", out int id))
        {
            return await RenderAdminAsync(user, null, "Некорректный идентификатор карты.", null, 400);
        }

        try
        {
            int affected = await _cards.DeleteAsync(id);
            if (affected == 0)
            {
                return await RenderAdminAsync(user, null, "Карта не найдена.", null, 404);
            }
        }
        catch (PostgresException exception)
        {
            return await RenderAdminAsync(user, null, DbErrorHelper.GetMessage(exception), null, 400);
        }

        return Redirect("/admin?notice=card_deleted");
    }

    [HttpPost("/admin/sets/create")]
    public async Task<IActionResult> CreateSet()
    {
        User? user = await GetCurrentUserAsync();
        if (user == null || user.Role != "Admin")
        {
            return Redirect("/login?error=access_denied");
        }

        Dictionary<string, string> form = await ReadFormAsync();
        if (!TryReadSetForm(form, out CardSet set, out string error))
        {
            return await RenderAdminAsync(user, null, error, set, 400);
        }

        try
        {
            await _sets.CreateAsync(set);
        }
        catch (PostgresException exception)
        {
            string message = DbErrorHelper.GetMessage(exception, "Сет с таким кодом уже существует.");
            return await RenderAdminAsync(user, null, message, set, 400);
        }

        return Redirect("/admin?notice=set_created");
    }

    [HttpPost("/admin/sets/delete")]
    public async Task<IActionResult> DeleteSet()
    {
        User? user = await GetCurrentUserAsync();
        if (user == null || user.Role != "Admin")
        {
            return Redirect("/login?error=access_denied");
        }

        Dictionary<string, string> form = await ReadFormAsync();
        if (!TryGetPositiveInt(form, "id", out int id))
        {
            return await RenderAdminAsync(user, null, "Некорректный идентификатор сета.", null, 400);
        }

        try
        {
            int affected = await _sets.DeleteAsync(id);
            if (affected == 0)
            {
                return await RenderAdminAsync(user, null, "Сет не найден.", null, 404);
            }
        }
        catch (PostgresException exception)
        {
            return await RenderAdminAsync(user, null, DbErrorHelper.GetMessage(exception), null, 400);
        }

        return Redirect("/admin?notice=set_deleted");
    }

    private async Task<IActionResult> RenderAdminAsync(
        User user,
        string? notice,
        string? error,
        CardSet? setForm,
        int statusCode)
    {
        // В админке нужны все карты, поэтому берем заведомо большой размер страницы
        CardPage page = await _cards.SearchAsync(new CardFilter { PageSize = 1000 });
        List<CardListItem> cards = page.Items;
        List<CardSet> sets = await _sets.GetAllOrderedAsync();

        Dictionary<string, object?> model = new()
        {
            ["PageTitle"] = "Админ-панель",
            ["Cards"] = cards,
            ["Sets"] = sets,
            ["CardCount"] = cards.Count,
            ["SetCount"] = sets.Count,
            ["SetForm"] = setForm ?? new CardSet { ReleaseDate = DateTime.Today },
            ["AdminNotice"] = notice ?? string.Empty,
            ["AdminError"] = error ?? string.Empty,
            ["SetsEmpty"] = sets.Count == 0
        };

        return RenderView("admin.html", model, user, statusCode);
    }

    private async Task<IActionResult> RenderCardFormAsync(
        User user,
        Card card,
        string mode,
        string? error,
        int statusCode)
    {
        List<CardSet> sets = await _sets.GetAllOrderedAsync();
        if (card.SetId <= 0 && sets.Count > 0)
        {
            card.SetId = sets[0].Id;
        }

        bool isEdit = string.Equals(mode, "edit", StringComparison.OrdinalIgnoreCase);
        Dictionary<string, object?> model = new()
        {
            ["PageTitle"] = isEdit ? "Редактирование карты" : "Новая карта",
            ["Card"] = card,
            ["SetOptions"] = BuildSetOptions(sets, card.SetId, includeAll: false),
            ["Error"] = error ?? string.Empty,
            ["IsEdit"] = isEdit,
            ["FormAction"] = isEdit ? "/admin/cards/edit" : "/admin/cards/create",
            ["IsCardForm"] = true,
            ["SetsEmpty"] = sets.Count == 0
        };

        return RenderView("admin-card-form.html", model, user, statusCode);
    }

    private bool TryReadCardForm(Dictionary<string, string> form, out Card card, out string error)
    {
        card = ReadCardTextFields(form);
        return TryReadCardNumbers(form, card, out error);
    }

    private Card ReadCardTextFields(Dictionary<string, string> form)
    {
        return new Card
        {
            CardNumber = FormValue(form, "cardNumber").Trim().ToUpperInvariant(),
            Name = FormValue(form, "name").Trim(),
            Subtitle = NullIfWhiteSpace(FormValue(form, "subtitle")),
            CardType = FormValue(form, "cardType").Trim(),
            Aspect = FormValue(form, "aspect").Trim(),
            Rarity = FormValue(form, "rarity").Trim(),
            Traits = NullIfWhiteSpace(FormValue(form, "traits")),
            AbilityText = _sanitizer.Sanitize(FormValue(form, "abilityText")),
            ImageUrl = NullIfWhiteSpace(FormValue(form, "imageUrl")),
            Artist = NullIfWhiteSpace(FormValue(form, "artist"))
        };
    }

    private static bool TryReadCardNumbers(Dictionary<string, string> form, Card card, out string error)
    {
        error = string.Empty;

        if (!int.TryParse(FormValue(form, "setId").Trim(), out int setId) || setId <= 0)
        {
            error = "Выберите сет для карты.";
            return false;
        }

        if (!int.TryParse(FormValue(form, "cost").Trim(), out int cost))
        {
            error = "Стоимость должна быть целым числом.";
            return false;
        }

        if (!TryParseOptionalInt(FormValue(form, "power"), 0, 30, out int? power))
        {
            error = "Сила должна быть целым числом от 0 до 30 или пустой.";
            return false;
        }

        if (!TryParseOptionalInt(FormValue(form, "hp"), 0, 50, out int? hp))
        {
            error = "HP должно быть целым числом от 0 до 50 или пустым.";
            return false;
        }

        card.SetId = setId;
        card.Cost = cost;
        card.Power = power;
        card.Hp = hp;
        return true;
    }

    private bool TryValidateCard(Card card, out string error)
    {
        if (!TryValidateCardIdentity(card, out error))
        {
            return false;
        }

        return TryValidateCardDetails(card, out error);
    }

    private bool TryValidateCardIdentity(Card card, out string error)
    {
        if (!_validation.IsValidCardNumber(card.CardNumber))
        {
            error = "Код карты должен иметь вид SOR-001 или IC27-061.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(card.Name) || card.Name.Length > 120)
        {
            error = "Название карты обязательно и не должно быть длиннее 120 символов.";
            return false;
        }

        if (card.Subtitle != null && card.Subtitle.Length > 120)
        {
            error = "Подзаголовок не должен быть длиннее 120 символов.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(card.CardType) || card.CardType.Length > 40)
        {
            error = "Тип карты обязателен и не должен быть длиннее 40 символов.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(card.Aspect) || card.Aspect.Length > 60)
        {
            error = "Аспект обязателен и не должен быть длиннее 60 символов.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(card.Rarity) || card.Rarity.Length > 30)
        {
            error = "Редкость обязательна и не должна быть длиннее 30 символов.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private bool TryValidateCardDetails(Card card, out string error)
    {
        if (!_validation.IsValidCost(card.Cost))
        {
            error = "Стоимость должна быть от 0 до 20.";
            return false;
        }

        if (!_validation.IsValidPower(card.Power) || !_validation.IsValidHp(card.Hp))
        {
            error = "Проверьте диапазоны силы (0–30) и HP (0–50).";
            return false;
        }

        if (card.Traits != null && card.Traits.Length > 200)
        {
            error = "Traits не должны быть длиннее 200 символов.";
            return false;
        }

        // Подставляем безопасный URL картинки: разрешаем только http(s) и пути от корня сайта
        if (card.ImageUrl != null)
        {
            if (card.ImageUrl.Length > 300)
            {
                error = "Ссылка на изображение не должна быть длиннее 300 символов.";
                return false;
            }

            if (!IsSafeImageUrl(card.ImageUrl))
            {
                error = "Ссылка на изображение должна начинаться с /, http:// или https://.";
                return false;
            }
        }

        if (card.Artist != null && card.Artist.Length > 100)
        {
            error = "Имя художника не должно быть длиннее 100 символов.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private bool TryReadSetForm(Dictionary<string, string> form, out CardSet set, out string error)
    {
        set = new CardSet
        {
            Code = FormValue(form, "code").Trim().ToUpperInvariant(),
            Name = FormValue(form, "name").Trim(),
            Description = NullIfWhiteSpace(FormValue(form, "description")),
            ReleaseDate = DateTime.Today
        };

        error = ValidateSet(set);
        DateTime releaseDate = DateTime.Today;
        if (error.Length == 0 && !TryParseReleaseDate(FormValue(form, "releaseDate"), out releaseDate))
        {
            error = "Укажите корректную дату выпуска.";
        }

        if (error.Length == 0)
        {
            set.ReleaseDate = releaseDate;
        }

        return error.Length == 0;
    }

    private string ValidateSet(CardSet set)
    {
        if (!_validation.IsValidSetCode(set.Code))
        {
            return "Код сета: 2–10 символов, только заглавные латинские буквы и цифры.";
        }

        if (string.IsNullOrWhiteSpace(set.Name) || set.Name.Length > 100)
        {
            return "Название сета обязательно и не должно быть длиннее 100 символов.";
        }

        if (set.Description != null && set.Description.Length > 2000)
        {
            return "Описание сета не должно быть длиннее 2000 символов.";
        }

        return string.Empty;
    }

    private static bool TryParseReleaseDate(string rawValue, out DateTime date)
    {
        return DateTime.TryParseExact(
            rawValue.Trim(),
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date);
    }

    private static bool TryParseOptionalInt(string rawValue, int min, int max, out int? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return true;
        }

        if (!int.TryParse(rawValue.Trim(), out int parsed) || parsed < min || parsed > max)
        {
            return false;
        }

        value = parsed;
        return true;
    }

    public static bool IsSafeImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return true;
        }

        string trimmed = url.Trim();
        // Запрещаем protocol-relative URL вида //evil.example и опасные схемы (javascript:, data:)
        if (trimmed.StartsWith('/') && !trimmed.StartsWith("//", StringComparison.Ordinal))
        {
            return true;
        }

        return trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    private static string? NullIfWhiteSpace(string value)
    {
        string trimmed = value.Trim();
        return trimmed.Length == 0 ? null : trimmed;
    }

    private static string GetNoticeMessage(string? notice)
    {
        switch (notice)
        {
            case "card_created":
                return "Карта добавлена.";
            case "card_updated":
                return "Карта сохранена.";
            case "card_deleted":
                return "Карта удалена.";
            case "set_created":
                return "Сет создан.";
            case "set_deleted":
                return "Сет удалён.";
            default:
                return string.Empty;
        }
    }
}
