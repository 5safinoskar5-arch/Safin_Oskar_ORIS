using SwU.Orm;

namespace SwU.WebApp.Models;

// DTO для каталога: карта вместе с кодом и названием сета из JOIN
public class CardListItem
{
    public int Id { get; set; }

    [DbColumn("set_id")]
    public int SetId { get; set; }

    [DbColumn("card_number")]
    public string CardNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Subtitle { get; set; }

    [DbColumn("card_type")]
    public string CardType { get; set; } = string.Empty;

    public string Aspect { get; set; } = string.Empty;

    public string Rarity { get; set; } = string.Empty;

    public int Cost { get; set; }

    public int? Power { get; set; }

    public int? Hp { get; set; }

    public string? Traits { get; set; }

    [DbColumn("ability_text")]
    public string? AbilityText { get; set; }

    [DbColumn("image_url")]
    public string? ImageUrl { get; set; }

    public string? Artist { get; set; }

    [DbColumn("set_code")]
    public string SetCode { get; set; } = string.Empty;

    [DbColumn("set_name")]
    public string SetName { get; set; } = string.Empty;

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

    public string RarityClass => string.IsNullOrWhiteSpace(Rarity)
        ? "common"
        : Rarity.ToLowerInvariant();
}
