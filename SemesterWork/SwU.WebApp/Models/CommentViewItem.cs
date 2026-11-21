using System.Globalization;
using SwU.Orm;

namespace SwU.WebApp.Models;

// DTO комментария: автора берем через JOIN с users
public class CommentViewItem
{
    public int Id { get; set; }

    [DbColumn("card_id")]
    public int CardId { get; set; }

    [DbColumn("user_id")]
    public int UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Username { get; set; } = string.Empty;

    [DbColumn("created_at")]
    public DateTime CreatedAt { get; set; }

    public string FormattedDate => CreatedAt.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
}
