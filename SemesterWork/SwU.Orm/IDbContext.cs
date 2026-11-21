namespace SwU.Orm;

public interface IDbContext
{
    Task<int> InsertAsync<T>(T entity) where T : class;
    Task<int> UpdateAsync<T>(T entity) where T : class;
    Task<int> DeleteAsync<T>(int id) where T : class;
    Task<T?> GetByIdAsync<T>(int id) where T : class, new();
    Task<List<T>> GetAllAsync<T>() where T : class, new();
    Task<List<T>> QueryAsync<T>(string sql, Dictionary<string, object?>? parameters = null) where T : class, new();
    Task<int> ExecuteAsync(string sql, Dictionary<string, object?>? parameters = null);
}
