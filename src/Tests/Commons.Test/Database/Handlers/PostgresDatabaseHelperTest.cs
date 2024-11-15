using System.Transactions;
using Commons.Database;
using Commons.Database.Handlers;

namespace Commons.Test.Database.Handlers;

[Category("IntegrationTests")]
public class PostgresDatabaseHelperTest
{
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await new PostgresTableHelper().CreateTableAsync<Models.TestDatabase>();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await new PostgresTableHelper().DeleteTableAsync<Models.TestDatabase>();
    }

    [TearDown]
    public async Task TearDown()
    {
        // Delete all rows from database
        await _helper.DeleteRows(nameof(Models.TestDatabase), null);
    }

    private readonly PostgresDatabaseHelper _helper = new();

    [TestCase(true)]
    [TestCase(false)]
    public async Task AssertThatSearchRowsReturnsRecordsIfDataIsPresent(bool hasData)
    {
        if (hasData)
        {
            await _helper.InsertData(nameof(Models.TestDatabase), new Dictionary<string, string>()
            {
                ["ColA"] = "1",
                ["ColB"] =  "abc"
            });
        }

        var searchResult = await _helper.SearchRows(nameof(Models.TestDatabase), null);
    
        Assert.That(searchResult.Any(), Is.EqualTo(hasData));
    }
    
    [TestCase("colA", "1", 1)]
    [TestCase("colA", "999", 0)]
    public async Task AssertThatSearchRowsReturnsRecordsGivenConditionsReturnsData(string whereClauseColumn, string whereClauseValue, int expectedCount)
    {
        await _helper.InsertData(nameof(Models.TestDatabase), new Dictionary<string, string>()
        {
            ["ColA"] = "1",
            ["ColB"] =  "abc"
        });
        
        await _helper.InsertData(nameof(Models.TestDatabase), new Dictionary<string, string>()
        {
            ["ColA"] = "2",
            ["ColB"] =  "def"
        });
        
        List<WhereClause> whereClause = [new WhereClause(whereClauseColumn, whereClauseValue)];
        
        var searchResult = await _helper.SearchRows(nameof(Models.TestDatabase), whereClause);
    
        Assert.That(searchResult.Count(), Is.EqualTo(expectedCount));
    }
}