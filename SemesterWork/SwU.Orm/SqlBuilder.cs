using System.Reflection;

namespace SwU.Orm;

public class SqlBuilder
{
    public string GetTableName<T>()
    {
        DbTableAttribute? attr = typeof(T).GetCustomAttribute<DbTableAttribute>();
        if (attr != null)
        {
            return attr.Name;
        }

        return typeof(T).Name.ToLowerInvariant();
    }

    public string GetPrimaryKeyColumn<T>()
    {
        PropertyInfo pkProp = GetPrimaryKeyProperty<T>();
        return GetColumnName(pkProp);
    }

    public string BuildSelectAll<T>()
    {
        string table = GetTableName<T>();
        List<string> columns = GetColumns<T>(includePrimaryKey: true);
        return $"SELECT {string.Join(", ", columns)} FROM {table}";
    }

    public string BuildSelectById<T>()
    {
        string table = GetTableName<T>();
        string pkCol = GetPrimaryKeyColumn<T>();
        List<string> columns = GetColumns<T>(includePrimaryKey: true);
        return $"SELECT {string.Join(", ", columns)} FROM {table} WHERE {pkCol} = @id";
    }

    public string BuildInsert<T>()
    {
        string table = GetTableName<T>();
        string pkCol = GetPrimaryKeyColumn<T>();
        List<PropertyInfo> props = GetMappedProperties<T>(includePrimaryKey: false);

        List<string> columns = new();
        List<string> paramNames = new();

        foreach (PropertyInfo prop in props)
        {
            string col = GetColumnName(prop);
            columns.Add(col);
            paramNames.Add("@" + col);
        }

        return $"INSERT INTO {table} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", paramNames)}) RETURNING {pkCol}";
    }

    public string BuildUpdate<T>()
    {
        string table = GetTableName<T>();
        string pkCol = GetPrimaryKeyColumn<T>();
        List<PropertyInfo> props = GetMappedProperties<T>(includePrimaryKey: false);

        List<string> assignments = new();
        foreach (PropertyInfo prop in props)
        {
            string col = GetColumnName(prop);
            assignments.Add($"{col} = @{col}");
        }

        return $"UPDATE {table} SET {string.Join(", ", assignments)} WHERE {pkCol} = @id";
    }

    public string BuildDelete<T>()
    {
        string table = GetTableName<T>();
        string pkCol = GetPrimaryKeyColumn<T>();
        return $"DELETE FROM {table} WHERE {pkCol} = @id";
    }

    internal static PropertyInfo GetPrimaryKeyProperty<T>()
    {
        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo prop in properties)
        {
            if (prop.IsDefined(typeof(DbPrimaryKeyAttribute), inherit: false))
            {
                return prop;
            }
        }

        throw new InvalidOperationException(
            $"В модели '{typeof(T).Name}' не найдено свойство с атрибутом [DbPrimaryKey].");
    }

    internal static List<PropertyInfo> GetMappedProperties<T>(bool includePrimaryKey)
    {
        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        List<PropertyInfo> result = new();

        foreach (PropertyInfo prop in properties)
        {
            if (!prop.CanRead || !prop.CanWrite || prop.GetIndexParameters().Length > 0)
            {
                continue;
            }

            bool isPk = prop.IsDefined(typeof(DbPrimaryKeyAttribute), inherit: false);
            if (isPk && !includePrimaryKey)
            {
                continue;
            }

            // Берем свойства с [DbColumn] или [DbPrimaryKey]
            if (isPk || prop.IsDefined(typeof(DbColumnAttribute), inherit: false))
            {
                result.Add(prop);
            }
        }

        if (result.Count == 0)
        {
            throw new InvalidOperationException(
                $"В модели '{typeof(T).Name}' не найдено свойств с атрибутами [DbColumn] / [DbPrimaryKey].");
        }

        return result;
    }

    internal static string GetColumnName(PropertyInfo prop)
    {
        DbColumnAttribute? attr = prop.GetCustomAttribute<DbColumnAttribute>();
        if (attr != null)
        {
            return attr.Name;
        }

        return prop.Name.ToLowerInvariant();
    }

    private static List<string> GetColumns<T>(bool includePrimaryKey)
    {
        List<PropertyInfo> props = GetMappedProperties<T>(includePrimaryKey);
        List<string> columns = new();
        foreach (PropertyInfo prop in props)
        {
            columns.Add(GetColumnName(prop));
        }

        return columns;
    }
}
