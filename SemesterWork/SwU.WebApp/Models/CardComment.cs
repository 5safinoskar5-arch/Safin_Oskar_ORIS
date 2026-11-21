using SwU.Orm;

namespace SwU.WebApp.Models;

[DbTable("comments")]
public class CardComment
{
    [DbPrimaryKey]
    public int Id { get; set; }

    [DbColumn("card_id")]
    public int CardId { get; set; }

    [DbColumn("user_id")]
    public int UserId { get; set; }

    [DbColumn("title")]
    public string Title { get; set; } = string.Empty;

    [DbColumn("body")]
    public string Body { get; set; } = string.Empty;

    [DbColumn("rating")]
    public int Rating { get; set; }

    [DbColumn("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
