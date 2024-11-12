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
        var combinedWhereClause = new List<string>();

        if (conditions != null)
        {
            combinedWhereClause.AddRange(conditions.Select(whereClause => $"{whereClause.ColumnName} = '{whereClause.Value}'"));
        }

        var whereClause = string.Join("AND", combinedWhereClause);
        var selectQuery = PostgresDatabaseConstants.SelectQuery.Replace("{tableName}", tableName).Replace("{whereClause}", whereClause);
        
        await using var dataSource = NpgsqlDataSource.Create(PostgresDatabaseConstants.ConnectionString);
        await using var npgsqlCommand = dataSource.CreateCommand(selectQuery);
        var reader = await npgsqlCommand.ExecuteReaderAsync();
        
        // ISSUE : GET ALL COLUMNS COUNT THEN GET THE COLUMNS
        while (await reader.ReadAsync())
        {
            resultList.Add($"{reader.GetInt32(0)} - {reader.GetString(1)}");
        }
        
        return resultList;
    }
}