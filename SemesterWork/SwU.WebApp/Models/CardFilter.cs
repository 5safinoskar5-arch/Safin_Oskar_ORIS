namespace SwU.WebApp.Models;

// Все фильтры каталога необязательные: null означает "фильтр не задан"
public class CardFilter
{
    public const int DefaultPageSize = 30;

    public string? Search { get; set; }

    public string? Aspect { get; set; }

    public string? CardType { get; set; }

    public string? Rarity { get; set; }

    public int? SetId { get; set; }

    public string? Trait { get; set; }

    public int? MinCost { get; set; }

    public int? MaxCost { get; set; }

    public int? MinPower { get; set; }

    public int? MaxPower { get; set; }

    public int? MinHp { get; set; }

    public int? MaxHp { get; set; }

    public string? Sort { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = DefaultPageSize;
}
