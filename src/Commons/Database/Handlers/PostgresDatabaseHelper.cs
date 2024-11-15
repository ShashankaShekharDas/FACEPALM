using Commons.Constants;
using Commons.Interfaces;
using Npgsql;

namespace Commons.Database.Handlers;

public sealed class PostgresDatabaseHelper : IDatabaseHelper
{
    public async Task<bool> InsertData(string tableName, Dictionary<string, string> columnsAndValues)
    {
        var columns = columnsAndValues.Keys.ToArray();
        var values = columnsAndValues.Values.ToArray();
        
        return await InsertData(tableName, columns, values);
    }

    public async Task<bool> InsertData(string tableName, string[] columnNames, string[] values)
    {
        var insertQuery = PostgresDatabaseConstants.InsertQuery
                                    .Replace("{tableName}", tableName)
                                    .Replace("{commaSeparatedColumns}", string.Join(",", columnNames))
                                    .Replace("{commaSeparatedPlaceHolders}", string.Join(",", values.Select(value => $"'{value}'")));
        
        await using var dataSource = NpgsqlDataSource.Create(PostgresDatabaseConstants.ConnectionString);
        await using var npgsqlCommand = dataSource.CreateCommand(insertQuery);
        
        var rowsModified = await npgsqlCommand.ExecuteNonQueryAsync();
        
        return rowsModified == 1;
    }
    
    public async Task<List<string>> SearchRows(string tableName, List<WhereClause>? conditions)
    {
        var resultList = new List<string>();
        var selectQuery = PostgresDatabaseConstants.SelectQuery.Replace("{tableName}", tableName).Replace("{whereClause}", WhereClause.GenerateWhereClause(conditions));
        
        await using var dataSource = NpgsqlDataSource.Create(PostgresDatabaseConstants.ConnectionString);
        await using var npgsqlCommand = dataSource.CreateCommand(selectQuery);
        var reader = await npgsqlCommand.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            resultList.Add($"{reader.GetInt32(0)} - {reader.GetString(1)}");
        }
        
        return resultList;
    }

    public async Task<bool> DeleteRows(string tableName, List<WhereClause>? conditions)
    {
        var deleteQuery = PostgresDatabaseConstants.DeleteQuery.Replace("{tableName}", tableName).Replace("{whereClause}", WhereClause.GenerateWhereClause(conditions));
        await using var dataSource = NpgsqlDataSource.Create(PostgresDatabaseConstants.ConnectionString);
        await using var npgsqlCommand = dataSource.CreateCommand(deleteQuery);
        var rowsDeleted = await npgsqlCommand.ExecuteNonQueryAsync();
        
        return rowsDeleted >= 1;
    }
}