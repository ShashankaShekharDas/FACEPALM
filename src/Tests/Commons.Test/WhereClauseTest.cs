using Commons.Database;

namespace Commons.Test;

public class WhereClauseTest
{
    [TestCase(null, null, "")]
    [TestCase(null, "123", "")]
    [TestCase("xyz", null, "WHERE xyz is null")]
    [TestCase("a", "b", "WHERE a = 'b'")]
    [TestCase("abc", "123", "WHERE abc = '123'")]
    [TestCase("abc", "123.456", "WHERE abc = '123.456'")]
    public void AssertThatWhereClausesAreGeneratedCorrectly(string? columnName, string? columnValue, string expectedWhereClause)
    {
        var whereClause = WhereClause.GenerateWhereClause([new WhereClause(columnName, columnValue)]);
        Assert.That(whereClause, Is.EqualTo(expectedWhereClause));
    }
}