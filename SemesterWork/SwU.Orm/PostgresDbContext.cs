using System.Globalization;
using System.Reflection;
using Npgsql;

namespace SwU.Orm;

public class PostgresDbContext : IDbContext
{
    private readonly string _connectionString;
    private readonly SqlBuilder _sqlBuilder;

    public PostgresDbContext(string connectionString, SqlBuilder? sqlBuilder = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Строка подключения к БД не может быть пустой.", nameof(connectionString));
        }

        _connectionString = connectionString;
        _sqlBuilder = sqlBuilder ?? new SqlBuilder();
    }

    public async Task<int> InsertAsync<T>(T entity) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        string sql = _sqlBuilder.BuildInsert<T>();
        Dictionary<string, object?> parameters = BuildEntityParameters(entity, includePrimaryKeyAsId: false);

        await using NpgsqlConnection connection = await OpenConnectionAsync();
        await using NpgsqlCommand command = CreateCommand(connection, sql, parameters);
        object? result = await command.ExecuteScalarAsync();

        int newId = Convert.ToInt32(result, CultureInfo.InvariantCulture);

        PropertyInfo pkProp = SqlBuilder.GetPrimaryKeyProperty<T>();
        if (pkProp.CanWrite && pkProp.PropertyType == typeof(int))
        {
            pkProp.SetValue(entity, newId);
        }

        return newId;
    }

    public async Task<int> UpdateAsync<T>(T entity) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        string sql = _sqlBuilder.BuildUpdate<T>();
        Dictionary<string, object?> parameters = BuildEntityParameters(entity, includePrimaryKeyAsId: true);

        return await ExecuteAsync(sql, parameters);
    }

    public async Task<int> DeleteAsync<T>(int id) where T : class
    {
        string sql = _sqlBuilder.BuildDelete<T>();
        Dictionary<string, object?> parameters = new()
        {
            ["@id"] = id
        };

        return await ExecuteAsync(sql, parameters);
    }

    public async Task<T?> GetByIdAsync<T>(int id) where T : class, new()
    {
        string sql = _sqlBuilder.BuildSelectById<T>();
        Dictionary<string, object?> parameters = new()
        {
            ["@id"] = id
        };

        List<T> items = await QueryAsync<T>(sql, parameters);
        return items.Count > 0 ? items[0] : null;
    }

    public async Task<List<T>> GetAllAsync<T>() where T : class, new()
    {
        string sql = _sqlBuilder.BuildSelectAll<T>();
        return await QueryAsync<T>(sql);
    }

    public async Task<List<T>> QueryAsync<T>(
        string sql,
        Dictionary<string, object?>? parameters = null) where T : class, new()
    {
        List<T> result = new();

        await using NpgsqlConnection connection = await OpenConnectionAsync();
        await using NpgsqlCommand command = CreateCommand(connection, sql, parameters);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

        // Один раз строим словарь "имя столбца -> свойство" для быстрого маппинга строк
        Dictionary<string, PropertyInfo> propertyMap = BuildPropertyMap<T>();

        while (await reader.ReadAsync())
        {
            T item = MapRow<T>(reader, propertyMap);
            result.Add(item);
        }

        return result;
    }

    public async Task<int> ExecuteAsync(string sql, Dictionary<string, object?>? parameters = null)
    {
        await using NpgsqlConnection connection = await OpenConnectionAsync();
        await using NpgsqlCommand command = CreateCommand(connection, sql, parameters);
        return await command.ExecuteNonQueryAsync();
    }

    private async Task<NpgsqlConnection> OpenConnectionAsync()
    {
        NpgsqlConnection connection = new(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static NpgsqlCommand CreateCommand(
        NpgsqlConnection connection,
        string sql,
        Dictionary<string, object?>? parameters)
    {
        NpgsqlCommand command = new(sql, connection);

        if (parameters == null)
        {
            return command;
        }

        foreach (KeyValuePair<string, object?> pair in parameters)
        {
            string name = pair.Key.StartsWith('@') ? pair.Key : "@" + pair.Key;
            command.Parameters.AddWithValue(name, pair.Value ?? DBNull.Value);
        }

        return command;
    }

    private static Dictionary<string, object?> BuildEntityParameters<T>(
        T entity,
        bool includePrimaryKeyAsId) where T : class
    {
        Dictionary<string, object?> parameters = new();
        List<PropertyInfo> props = SqlBuilder.GetMappedProperties<T>(includePrimaryKey: false);

        foreach (PropertyInfo prop in props)
        {
            string col = SqlBuilder.GetColumnName(prop);
            parameters["@" + col] = prop.GetValue(entity);
        }

        if (includePrimaryKeyAsId)
        {
            PropertyInfo pkProp = SqlBuilder.GetPrimaryKeyProperty<T>();
            parameters["@id"] = pkProp.GetValue(entity);
        }

        return parameters;
    }

    private static Dictionary<string, PropertyInfo> BuildPropertyMap<T>()
    {
        Dictionary<string, PropertyInfo> map = new(StringComparer.OrdinalIgnoreCase);
        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (PropertyInfo prop in properties)
        {
            if (!prop.CanWrite || prop.GetIndexParameters().Length > 0)
            {
                continue;
            }

            DbColumnAttribute? colAttr = prop.GetCustomAttribute<DbColumnAttribute>();
            if (colAttr != null)
            {
                map[colAttr.Name] = prop;
            }

            if (!map.ContainsKey(prop.Name))
            {
                map[prop.Name] = prop;
            }
        }

        return map;
    }

    private static T MapRow<T>(NpgsqlDataReader reader, Dictionary<string, PropertyInfo> propertyMap)
        where T : class, new()
    {
        T entity = new();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);
            if (!propertyMap.TryGetValue(columnName, out PropertyInfo? prop))
            {
                continue;
            }

            object rawValue = reader.GetValue(i);
            if (rawValue == DBNull.Value)
            {
                if (!prop.PropertyType.IsValueType || Nullable.GetUnderlyingType(prop.PropertyType) != null)
                {
                    prop.SetValue(entity, null);
                }

                continue;
            }

            object? converted = ConvertValue(rawValue, prop.PropertyType);
            prop.SetValue(entity, converted);
        }

        return entity;
    }

    private static object? ConvertValue(object rawValue, Type targetType)
    {
        Type effectiveType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (effectiveType.IsInstanceOfType(rawValue))
        {
            return rawValue;
        }

        // Npgsql 8 возвращает PostgreSQL DATE как DateOnly; поддерживаем обе стороны преобразования
        if (effectiveType == typeof(DateTime) && rawValue is DateOnly dateOnly)
        {
            return dateOnly.ToDateTime(TimeOnly.MinValue);
        }

        if (effectiveType == typeof(DateOnly) && rawValue is DateTime dateTime)
        {
            return DateOnly.FromDateTime(dateTime);
        }

        if (effectiveType.IsEnum)
        {
            return rawValue is string enumText
                ? Enum.Parse(effectiveType, enumText, ignoreCase: true)
                : Enum.ToObject(effectiveType, rawValue);
        }

        return Convert.ChangeType(rawValue, effectiveType, CultureInfo.InvariantCulture);
    }
}
