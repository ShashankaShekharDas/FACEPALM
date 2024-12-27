using Commons.Constants;
using Commons.Database;
using Commons.Database.Handlers;
using Commons.Test.Models;

namespace Commons.Test.Database.Handlers;

[Category("IntegrationTests")]
public class PostgresDatabaseHelperTest
{
    private readonly PostgresDatabaseHelper _helper = new();

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
        await _helper.DeleteRows(nameof(TestDatabase), null);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task AssertThatSearchRowsReturnsRecordsIfDataIsPresent(bool hasData)
    {
        if (hasData)
            await _helper.InsertData(nameof(TestDatabase), new Dictionary<string, string>
            {
                ["ColA"] = "1",
                ["ColB"] = "abc"
            });

        var searchResult = await _helper.SearchRows(nameof(TestDatabase), null);

        Assert.That(searchResult.Any(), Is.EqualTo(hasData));
    }

    [TestCase("colA", "1", 1)]
    [TestCase("colA", "999", 0)]
    public async Task AssertThatSearchRowsReturnsRecordsGivenConditionsReturnsData(string whereClauseColumn,
        string whereClauseValue, int expectedCount)
    {
        await _helper.InsertData(nameof(TestDatabase), new Dictionary<string, string>
        {
            ["ColA"] = "1",
            ["ColB"] = "abc"
        });

        await _helper.InsertData(nameof(TestDatabase), new Dictionary<string, string>
        {
            ["ColA"] = "2",
            ["ColB"] = "def"
        });

        List<WhereClause> whereClause = [new(whereClauseColumn, whereClauseValue, DatabaseOperator.Equal)];

        var searchResult = await _helper.SearchRows(nameof(TestDatabase), whereClause);

        Assert.That(searchResult, Has.Count.EqualTo(expectedCount));
    }
}