using SwU.Orm;

namespace SwU.Tests;

[TestClass]
public class SqlBuilderTests
{
    private readonly SqlBuilder _builder = new();

    [TestMethod]
    public void GetTableName_And_PrimaryKey_UseAttributes()
    {
        Assert.AreEqual("test_cards", _builder.GetTableName<TestCard>());
        Assert.AreEqual("id", _builder.GetPrimaryKeyColumn<TestCard>());
    }

    [TestMethod]
    public void BuildSelectAll_ContainsAllMappedColumns()
    {
        string sql = _builder.BuildSelectAll<TestCard>();

        StringAssert.StartsWith(sql, "SELECT ");
        StringAssert.Contains(sql, "FROM test_cards");
        StringAssert.Contains(sql, "id");
        StringAssert.Contains(sql, "card_number");
        StringAssert.Contains(sql, "name");
        StringAssert.Contains(sql, "cost");
    }

    [TestMethod]
    public void BuildSelectById_FiltersByPrimaryKeyParameter()
    {
        string sql = _builder.BuildSelectById<TestCard>();

        StringAssert.Contains(sql, "FROM test_cards");
        StringAssert.EndsWith(sql, "WHERE id = @id");
    }

    [TestMethod]
    public void BuildInsert_SkipsPrimaryKey_AndReturnsGeneratedId()
    {
        string sql = _builder.BuildInsert<TestCard>();

        StringAssert.StartsWith(sql, "INSERT INTO test_cards (");
        StringAssert.Contains(sql, "@card_number");
        StringAssert.Contains(sql, "@name");
        StringAssert.Contains(sql, "@cost");
        StringAssert.EndsWith(sql, "RETURNING id");
        Assert.IsFalse(sql.Contains("@id", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildUpdate_AssignsEveryColumn_AndFiltersById()
    {
        string sql = _builder.BuildUpdate<TestCard>();

        StringAssert.StartsWith(sql, "UPDATE test_cards SET ");
        StringAssert.Contains(sql, "card_number = @card_number");
        StringAssert.Contains(sql, "name = @name");
        StringAssert.Contains(sql, "cost = @cost");
        StringAssert.EndsWith(sql, "WHERE id = @id");
    }

    [TestMethod]
    public void BuildDelete_FiltersById()
    {
        string sql = _builder.BuildDelete<TestCard>();

        Assert.AreEqual("DELETE FROM test_cards WHERE id = @id", sql);
    }

    [TestMethod]
    public void BuildSelectAll_WithoutDbTableAttribute_UsesLowercaseClassName()
    {
        string sql = _builder.BuildSelectAll<PlainModel>();

        StringAssert.Contains(sql, "FROM plainmodel");
    }

    [DbTable("test_cards")]
    private sealed class TestCard
    {
        [DbPrimaryKey]
        public int Id { get; set; }

        [DbColumn("card_number")]
        public string CardNumber { get; set; } = string.Empty;

        [DbColumn("name")]
        public string Name { get; set; } = string.Empty;

        [DbColumn("cost")]
        public int Cost { get; set; }
    }

    private sealed class PlainModel
    {
        [DbPrimaryKey]
        public int Id { get; set; }

        [DbColumn("title")]
        public string Title { get; set; } = string.Empty;
    }
}
