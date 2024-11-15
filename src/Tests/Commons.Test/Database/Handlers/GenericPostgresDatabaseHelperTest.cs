using Commons.Database;
using Commons.Database.Handlers;
using Commons.Test.Models;

namespace Commons.Test.Database.Handlers;

public class GenericPostgresDatabaseHelperTest
{
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await new PostgresTableHelper().CreateTableAsync<TestDatabase>();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await new PostgresTableHelper().DeleteTableAsync<TestDatabase>();
    }

    [TearDown]
    public async Task TearDown()
    {
        // Delete all rows from database
        await _helper.DeleteRows( null);
    }

    private readonly GenericPostgresDatabaseHelper<TestDatabase> _helper = new();

    [TestCase(true)]
    [TestCase(false)]
    public async Task AssertThatSearchRowsReturnsRecordsIfDataIsPresent(bool hasData)
    {
        if (hasData)
        {
            var rowsToInsert = new List<TestDatabase>
            {
                new TestDatabase(1, "abc"),
            };
            await _helper.InsertData(rowsToInsert);
        }

        var searchResult = await _helper.SearchRows(null, TestDatabase.Deserialize);
    
        Assert.That(searchResult.Any(), Is.EqualTo(hasData));
    }
    
    [TestCase("colA", "1", 1)]
    [TestCase("colA", "999", 0)]
    public async Task AssertThatSearchRowsReturnsRecordsGivenConditionsReturnsData(string whereClauseColumn, string whereClauseValue, int expectedCount)
    {
        var rowsToInsert = new List<TestDatabase>
        {
            new TestDatabase(1, "abc"),
            new TestDatabase(2, "def"),
        };
        await _helper.InsertData(rowsToInsert);
        
        List<WhereClause> whereClause = [new WhereClause(whereClauseColumn, whereClauseValue)];
        
        var searchResult = await _helper.SearchRows(whereClause, TestDatabase.Deserialize);
    
        Assert.That(searchResult.Count(), Is.EqualTo(expectedCount));
    }
}