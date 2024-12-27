using Commons.Constants;
using Commons.Database;
using Commons.Database.Handlers;
using Commons.Test.Models;

namespace Commons.Test.Database.Handlers;

public class GenericPostgresDatabaseHelperTest
{
    private readonly GenericPostgresDatabaseHelper<TestDatabase> _helper = new();

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await PostgresTableHelper<TestDatabase>.CreateTableAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await PostgresTableHelper<TestDatabase>.DeleteTableAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        // Delete all rows from database
        await _helper.DeleteRows(null);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task AssertThatSearchRowsReturnsRecordsIfDataIsPresent(bool hasData)
    {
        if (hasData)
        {
            var rowsToInsert = new List<TestDatabase>
            {
                new(1, "abc")
            };
            await _helper.InsertData(rowsToInsert);
        }

        var searchResult = await _helper.SearchRows(null, TestDatabase.Deserialize);

        Assert.That(searchResult.Any(), Is.EqualTo(hasData));
    }

    [TestCase("colA", "1", 1)]
    [TestCase("colA", "999", 0)]
    public async Task AssertThatSearchRowsReturnsRecordsGivenConditionsReturnsData(string whereClauseColumn,
        string whereClauseValue, int expectedCount)
    {
        var rowsToInsert = new List<TestDatabase>
        {
            new(1, "abc"),
            new(2, "def")
        };
        await _helper.InsertData(rowsToInsert);

        List<WhereClause> whereClause = [new(whereClauseColumn, whereClauseValue, DatabaseOperator.Equal)];

        var searchResult = await _helper.SearchRows(whereClause, TestDatabase.Deserialize);

        Assert.That(searchResult, Has.Count.EqualTo(expectedCount));
    }
}