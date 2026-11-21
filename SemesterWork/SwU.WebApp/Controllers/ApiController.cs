using Npgsql;
using SwU.HttpFramework.Mvc;
using SwU.HttpFramework.Mvc.Results;
using SwU.HttpFramework.Templating;
using SwU.WebApp.Models;
using SwU.WebApp.Repositories;
using SwU.WebApp.Services;

namespace SwU.WebApp.Controllers;

[Controller]
public class ApiController : AppController
{
    private readonly CardRepository _cards;

    public ApiController(CardRepository cards, AuthService authService, ITemplateEngine templateEngine)
        : base(templateEngine, authService)
    {
        _cards = cards;
    }

    [HttpGet("/api/cards")]
    public async Task<IActionResult> GetCards()
    {
        CardFilter filter = BuildCardFilter();
        CardPage page = await _cards.SearchAsync(filter);

        // JSON: { items, totalCount, page, pageSize, totalPages }
        return Json(page);
    }

    [HttpPost("/api/admin/cards/delete")]
    public async Task<IActionResult> DeleteCard()
    {
        // Роль всегда перечитывается из БД внутри GetCurrentUserAsync
        User? user = await GetCurrentUserAsync();
        if (user == null || user.Role != "Admin")
        {
            return Json(new { error = "Недостаточно прав для удаления карты." }, 403);
        }

        Dictionary<string, string> form = await ReadFormAsync();
        if (!TryGetPositiveInt(form, "id", out int id))
        {
            return Json(new { error = "Некорректный идентификатор карты." }, 400);
        }

        try
        {
            int affected = await _cards.DeleteAsync(id);
            if (affected == 0)
            {
                return Json(new { error = "Карта не найдена." }, 404);
            }
        }
        catch (PostgresException exception)
        {
            return Json(new { error = DbErrorHelper.GetMessage(exception) }, 400);
        }

        return Json(new { success = true });
    }
}
