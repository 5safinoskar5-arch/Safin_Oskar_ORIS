using SwU.Orm;

namespace SwU.WebApp.Models;

// DTO для SELECT COUNT(*): мини-ORM маппит колонку "total" в это свойство
public class CountRow
{
    [DbColumn("total")]
    public int Total { get; set; }
}
