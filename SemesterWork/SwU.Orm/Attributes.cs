namespace SwU.Orm;

[AttributeUsage(AttributeTargets.Class)]
public sealed class DbTableAttribute : Attribute
{
    public string Name { get; }

    public DbTableAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Имя таблицы не может быть пустым.", nameof(name));
        }

        Name = name;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class DbColumnAttribute : Attribute
{
    public string Name { get; }

    public DbColumnAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Имя столбца не может быть пустым.", nameof(name));
        }

        Name = name;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class DbPrimaryKeyAttribute : Attribute
{
}
