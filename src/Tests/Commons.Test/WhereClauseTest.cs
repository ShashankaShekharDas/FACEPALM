using Commons.Constants;
using Commons.Database;

namespace Commons.Test
{
    public class WhereClauseTest
    {
        [TestCase(null, null, "", DatabaseOperator.Equal)]
        [TestCase(null, "123", "", DatabaseOperator.Equal)]
        [TestCase("xyz", null, "WHERE xyz is null", DatabaseOperator.Null)]
        [TestCase("a", "b", "WHERE a = 'b'", DatabaseOperator.Equal)]
        [TestCase("abc", "123", "WHERE abc = '123'", DatabaseOperator.Equal)]
        [TestCase("abc", "123.456", "WHERE abc = '123.456'", DatabaseOperator.Equal)]
        public void AssertThatWhereClausesAreGeneratedCorrectly(string? columnName, string? columnValue,
            string expectedWhereClause, DatabaseOperator dbOperator)
        {
            var whereClause = WhereClause.GenerateWhereClause([new WhereClause(columnName, columnValue, dbOperator)]);
            Assert.That(whereClause, Is.EqualTo(expectedWhereClause));
        }
    }
}