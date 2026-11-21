using SwU.Orm;
using SwU.WebApp.Models;

namespace SwU.WebApp.Repositories;

public class UserRepository : BaseRepository<User>
{
    public UserRepository(IDbContext db) : base(db)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        const string sql = @"
SELECT id, username, email, password_hash, role, created_at
FROM users
WHERE username = @username
LIMIT 1";

        List<User> users = await Db.QueryAsync<User>(sql, new Dictionary<string, object?>
        {
            ["@username"] = username.Trim()
        });

        return users.Count > 0 ? users[0] : null;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        const string sql = @"
SELECT id, username, email, password_hash, role, created_at
FROM users
WHERE email = @email
LIMIT 1";

        List<User> users = await Db.QueryAsync<User>(sql, new Dictionary<string, object?>
        {
            ["@email"] = email.Trim()
        });

        return users.Count > 0 ? users[0] : null;
    }
}
