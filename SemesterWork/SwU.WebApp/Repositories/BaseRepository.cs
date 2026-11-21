using SwU.Orm;

namespace SwU.WebApp.Repositories;

public abstract class BaseRepository<T> : IRepository<T> where T : class, new()
{
    protected readonly IDbContext Db;

    protected BaseRepository(IDbContext db)
    {
        Db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public virtual Task<T?> GetByIdAsync(int id)
    {
        return Db.GetByIdAsync<T>(id);
    }

    public virtual Task<List<T>> GetAllAsync()
    {
        return Db.GetAllAsync<T>();
    }

    public virtual Task<int> CreateAsync(T entity)
    {
        return Db.InsertAsync(entity);
    }

    public virtual Task<int> UpdateAsync(T entity)
    {
        return Db.UpdateAsync(entity);
    }

    public virtual Task<int> DeleteAsync(int id)
    {
        return Db.DeleteAsync<T>(id);
    }
}
