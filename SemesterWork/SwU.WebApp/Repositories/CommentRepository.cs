using SwU.Orm;
using SwU.WebApp.Models;

namespace SwU.WebApp.Repositories;

public class CommentRepository : BaseRepository<CardComment>
{
    public CommentRepository(IDbContext db) : base(db)
    {
    }

    public async Task<List<CommentViewItem>> GetByCardIdAsync(int cardId)
    {
        const string sql = @"
SELECT cm.id, cm.card_id, cm.user_id, cm.title, cm.body, cm.rating,
       cm.created_at, u.username
FROM comments cm
JOIN users u ON u.id = cm.user_id
WHERE cm.card_id = @cardId
ORDER BY cm.created_at DESC, cm.id DESC";

        return await Db.QueryAsync<CommentViewItem>(sql, new Dictionary<string, object?>
        {
            ["@cardId"] = cardId
        });
    }
}
