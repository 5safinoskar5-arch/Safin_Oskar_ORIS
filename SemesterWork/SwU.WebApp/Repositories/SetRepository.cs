using SwU.Orm;
using SwU.WebApp.Models;

namespace SwU.WebApp.Repositories;

public class SetRepository : BaseRepository<CardSet>
{
    public SetRepository(IDbContext db) : base(db)
    {
    }

    public async Task<List<CardSet>> GetAllOrderedAsync()
    {
        const string sql = @"
SELECT id, code, name, release_date, description
FROM sets
ORDER BY release_date DESC, code ASC";

        return await Db.QueryAsync<CardSet>(sql);
    }
}
