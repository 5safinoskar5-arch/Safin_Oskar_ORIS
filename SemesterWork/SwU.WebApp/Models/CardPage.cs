namespace SwU.WebApp.Models;

// Одна страница каталога: элементы + общее число найденных карт для пагинации
public class CardPage
{
    public List<CardListItem> Items { get; set; } = new();

    public int TotalCount { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = CardFilter.DefaultPageSize;

    public int TotalPages => PageSize <= 0 ? 0 : (TotalCount + PageSize - 1) / PageSize;
}
