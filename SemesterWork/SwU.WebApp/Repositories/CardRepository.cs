using SwU.Orm;
using SwU.WebApp.Models;

namespace SwU.WebApp.Repositories;

public class CardRepository : BaseRepository<Card>
{
    public CardRepository(IDbContext db) : base(db)
    {
    }

    public async Task<CardPage> SearchAsync(CardFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        List<string> conditions = new();
        Dictionary<string, object?> parameters = new();
        AddFilters(conditions, parameters, filter);

        string where = conditions.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", conditions);
        string orderBy = BuildOrderBy(filter.Sort);

        string countSql = $@"
SELECT COUNT(*) AS total
FROM cards c
JOIN sets s ON s.id = c.set_id
{where}";

        List<CountRow> countRows = await Db.QueryAsync<CountRow>(countSql, parameters);
        int totalCount = countRows.Count > 0 ? countRows[0].Total : 0;

        int page = filter.Page < 1 ? 1 : filter.Page;
        int pageSize = filter.PageSize < 1 ? CardFilter.DefaultPageSize : filter.PageSize;

        // LIMIT/OFFSET передаем параметрами: значения не попадают в текст SQL
        int totalPages = (totalCount + pageSize - 1) / pageSize;
        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }
        else if (totalPages == 0)
        {
            page = 1;
        }

        parameters["@pageSize"] = pageSize;
        parameters["@offset"] = (page - 1) * pageSize;

        string sql = $@"
SELECT c.id, c.set_id, c.card_number, c.name, c.subtitle, c.card_type, c.aspect,
       c.rarity, c.cost, c.power, c.hp, c.traits, c.ability_text, c.image_url,
       c.artist, s.code AS set_code, s.name AS set_name
FROM cards c
JOIN sets s ON s.id = c.set_id
{where}
{orderBy}
LIMIT @pageSize OFFSET @offset";

        List<CardListItem> items = await Db.QueryAsync<CardListItem>(sql, parameters);

        return new CardPage
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static void AddFilters(
        List<string> conditions,
        Dictionary<string, object?> parameters,
        CardFilter filter)
    {
        AddTextFilters(conditions, parameters, filter);
        AddRangeFilters(conditions, parameters, filter);
    }

    private static void AddTextFilters(
        List<string> conditions,
        Dictionary<string, object?> parameters,
        CardFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            // ILIKE ищем по названию, подзаголовку, номеру и traits
            conditions.Add("(c.name ILIKE @search OR c.subtitle ILIKE @search OR c.card_number ILIKE @search OR c.traits ILIKE @search)");
            parameters["@search"] = "%" + filter.Search.Trim() + "%";
        }

        if (!string.IsNullOrWhiteSpace(filter.Aspect))
        {
            // % вокруг значения поддерживает двойные аспекты вида "Command, Villainy"
            conditions.Add("c.aspect ILIKE @aspect");
            parameters["@aspect"] = "%" + filter.Aspect.Trim() + "%";
        }

        if (!string.IsNullOrWhiteSpace(filter.CardType))
        {
            conditions.Add("c.card_type ILIKE @cardType");
            parameters["@cardType"] = filter.CardType.Trim();
        }

        if (!string.IsNullOrWhiteSpace(filter.Rarity))
        {
            conditions.Add("c.rarity ILIKE @rarity");
            parameters["@rarity"] = filter.Rarity.Trim();
        }

        if (filter.SetId.HasValue && filter.SetId.Value > 0)
        {
            conditions.Add("c.set_id = @setId");
            parameters["@setId"] = filter.SetId.Value;
        }

        if (!string.IsNullOrWhiteSpace(filter.Trait))
        {
            conditions.Add("c.traits ILIKE @trait");
            parameters["@trait"] = "%" + filter.Trait.Trim() + "%";
        }
    }

    private static void AddRangeFilters(
        List<string> conditions,
        Dictionary<string, object?> parameters,
        CardFilter filter)
    {
        AddRange(conditions, parameters, "c.cost", "@minCost", "@maxCost", filter.MinCost, filter.MaxCost);
        AddRange(conditions, parameters, "c.power", "@minPower", "@maxPower", filter.MinPower, filter.MaxPower);
        AddRange(conditions, parameters, "c.hp", "@minHp", "@maxHp", filter.MinHp, filter.MaxHp);
    }

    // Имена колонок и параметров заданы в коде, пользовательские значения идут только в parameters
    private static void AddRange(
        List<string> conditions,
        Dictionary<string, object?> parameters,
        string column,
        string minParameter,
        string maxParameter,
        int? min,
        int? max)
    {
        if (min.HasValue)
        {
            conditions.Add(column + " >= " + minParameter);
            parameters[minParameter] = min.Value;
        }

        if (max.HasValue)
        {
            conditions.Add(column + " <= " + maxParameter);
            parameters[maxParameter] = max.Value;
        }
    }

    // Сортировка только по белому списку, пользовательская строка в SQL не попадает
    private static string BuildOrderBy(string? sort)
    {
        switch (sort)
        {
            case "cost_asc":
                return "ORDER BY c.cost ASC, c.name ASC";
            case "cost_desc":
                return "ORDER BY c.cost DESC, c.name ASC";
            case "hp_desc":
                return "ORDER BY c.hp DESC NULLS LAST, c.name ASC";
            case "name":
                return "ORDER BY c.name ASC, c.card_number ASC";
            default:
                return "ORDER BY c.card_number ASC, c.id ASC";
        }
    }
}
