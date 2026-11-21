using SwU.Orm;

namespace SwU.WebApp.Models;

[DbTable("users")]
public class User
{
    [DbPrimaryKey]
    public int Id { get; set; }

    [DbColumn("username")]
    public string Username { get; set; } = string.Empty;

    [DbColumn("email")]
    public string Email { get; set; } = string.Empty;

    [DbColumn("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [DbColumn("role")]
    public string Role { get; set; } = "User";

    [DbColumn("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
