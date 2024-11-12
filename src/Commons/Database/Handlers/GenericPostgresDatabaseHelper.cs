using Commons.Constants;
using Commons.Interfaces;
using Commons.Models;
using Npgsql;

namespace Commons.Database.Handlers;

public class GenericPostgresDatabaseHelper<T> : IGenericDatabaseHelper<T> 
    where T : BaseDatabaseModels
{
    public async Task<bool> InsertData(List<T> rowsToInsert)
    {
        if (rowsToInsert.Count == 0)
        {
            return false;
        }
        
        var tableName = typeof(T).Name;
        var columns = new List<string>();
        var values = new List<string>();
        
        foreach (var row in rowsToInsert)
        {
            foreach (var property in row.GetType().GetProperties())
            {
                var value = property.GetValue(row);
                
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    continue;
                }
                
                columns.Add(property.Name);
                
                if (property.PropertyType.IsEnum)
                {
                    value = Convert.ChangeType(value, Enum.GetUnderlyingType(property.PropertyType));
                }
                values.Add(value.ToString()!);
            }
        }
        
        var insertQuery = PostgresDatabaseConstants.InsertQuery
            .Replace("{tableName}", tableName)
            .Replace("{commaSeparatedColumns}", string.Join(",", columns))
            .Replace("{commaSeparatedPlaceHolders}", string.Join(",", values.Select(value => $"'{value}'")));

        await using var dataSource = NpgsqlDataSource.Create(PostgresDatabaseConstants.ConnectionString);
        await using var npgsqlCommand = dataSource.CreateCommand(insertQuery);
        
        var rowsModified = await npgsqlCommand.ExecuteNonQueryAsync();
        
        return rowsModified == 1;
    }

    public async Task<List<T>> SearchRows(List<WhereClause>? conditions)
    {
        var resultList = new List<T>();
        var combinedWhereClause = new List<string>();

        if (conditions != null)
        {
            combinedWhereClause.AddRange(conditions.Select(whereClause => $"{whereClause.ColumnName} = '{whereClause.Value}'"));
        }
        
        var tableName = typeof(T).Name;
        var whereClause = string.Empty;
        if (combinedWhereClause.Any())
        {
            whereClause = $"WHERE {string.Join("AND", combinedWhereClause)}";
        }
        var selectQuery = PostgresDatabaseConstants.SelectQuery.Replace("{tableName}", tableName).Replace("{whereClause}", whereClause);
        
        await using var dataSource = NpgsqlDataSource.Create(PostgresDatabaseConstants.ConnectionString);
        await using var npgsqlCommand = dataSource.CreateCommand(selectQuery);
        var reader = await npgsqlCommand.ExecuteReaderAsync();

        // ISSUE : GET ALL COLUMNS COUNT THEN GET THE COLUMNS
        while (await reader.ReadAsync())
        {
            switch (tableName)
            {
                case "Test":
                    resultList.Add(Test.Deserialize(reader) as T);
                    break;
                default:
                    resultList.Add(ChunkInformation.Deserialize(reader) as T);
                    break;
            }
        }
        
        return resultList;
    }
}