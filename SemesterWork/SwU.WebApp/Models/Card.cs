using SwU.Orm;

namespace SwU.WebApp.Models;

[DbTable("cards")]
public class Card
{
    [DbPrimaryKey]
    public int Id { get; set; }

    [DbColumn("set_id")]
    public int SetId { get; set; }

    [DbColumn("card_number")]
    public string CardNumber { get; set; } = string.Empty;

    [DbColumn("name")]
    public string Name { get; set; } = string.Empty;

    [DbColumn("subtitle")]
    public string? Subtitle { get; set; }

    [DbColumn("card_type")]
    public string CardType { get; set; } = string.Empty;

    [DbColumn("aspect")]
    public string Aspect { get; set; } = string.Empty;

    [DbColumn("rarity")]
    public string Rarity { get; set; } = string.Empty;

    [DbColumn("cost")]
    public int Cost { get; set; }

    [DbColumn("power")]
    public int? Power { get; set; }

    [DbColumn("hp")]
    public int? Hp { get; set; }

    [DbColumn("traits")]
    public string? Traits { get; set; }

    [DbColumn("ability_text")]
    public string? AbilityText { get; set; }

    [DbColumn("image_url")]
    public string? ImageUrl { get; set; }

    [DbColumn("artist")]
    public string? Artist { get; set; }

    public bool HasPower => Power.HasValue;

    public bool HasHp => Hp.HasValue;

    public string PowerText => Power.HasValue ? Power.Value.ToString() : "—";

    public string HpText => Hp.HasValue ? Hp.Value.ToString() : "—";

    public string ImageUrlDisplay => IsSafeUrl(ImageUrl)
        ? ImageUrl!.Trim()
        : "/images/cards/placeholder.svg";

    private static bool IsSafeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        string trimmed = url.Trim();
        if (trimmed.StartsWith('/') && !trimmed.StartsWith("//", StringComparison.Ordinal))
        {
            return true;
        }

        return trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }
}
