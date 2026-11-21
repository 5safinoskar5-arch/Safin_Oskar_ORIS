using System.Globalization;
using SwU.Orm;

namespace SwU.WebApp.Models;

[DbTable("sets")]
public class CardSet
{
    [DbPrimaryKey]
    public int Id { get; set; }

    [DbColumn("code")]
    public string Code { get; set; } = string.Empty;

    [DbColumn("name")]
    public string Name { get; set; } = string.Empty;

    [DbColumn("release_date")]
    public DateTime ReleaseDate { get; set; } = DateTime.Today;

    [DbColumn("description")]
    public string? Description { get; set; }

    public string ReleaseDateText => ReleaseDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

    public string ReleaseDateValue => ReleaseDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
