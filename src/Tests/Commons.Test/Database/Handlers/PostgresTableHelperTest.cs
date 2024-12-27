using Commons.Constants;
using Commons.Database.Handlers;
using Commons.Test.Models;
using Npgsql;

namespace Commons.Test.Database.Handlers;

public class PostgresTableHelperTest
{
    private const string CheckIfTableExistsQuery =
        "SELECT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = '{tableName}' );";

    [Test]
    [Order(1)]
    public async Task AsserThatTableCreateStatementCreatesTable()
    {
        await PostgresTableHelper<TestDatabase>.CreateTableAsync();

        await using var dataSource = NpgsqlDataSource.Create(PostgresDatabaseConstants.ConnectionString);
        await using var npgsqlCommand =
            dataSource.CreateCommand(CheckIfTableExistsQuery.Replace("{tableName}", nameof(TestDatabase).ToLower()));
        var reader = await npgsqlCommand.ExecuteReaderAsync();
        var tableExists = false;

        while (await reader.ReadAsync()) tableExists = reader.GetBoolean(0);

        Assert.That(tableExists, Is.EqualTo(true));
    }

    [Test]
    [Order(2)]
    public async Task AssertThatDropStatementDropsTable()
    {
        await PostgresTableHelper<TestDatabase>.DeleteTableAsync();

        await using var dataSource = NpgsqlDataSource.Create(PostgresDatabaseConstants.ConnectionString);
        await using var npgsqlCommand =
            dataSource.CreateCommand(CheckIfTableExistsQuery.Replace("{tableName}", nameof(TestDatabase).ToLower()));
        var reader = await npgsqlCommand.ExecuteReaderAsync();
        var tableExists = true;

        while (await reader.ReadAsync()) tableExists = reader.GetBoolean(0);

        Assert.That(tableExists, Is.EqualTo(false));
    }
}