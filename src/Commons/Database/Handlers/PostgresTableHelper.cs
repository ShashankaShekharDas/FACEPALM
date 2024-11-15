using Commons.Constants;
using Commons.Interfaces;

namespace Commons.Database.Handlers;

using Npgsql;
using System;
using System.Text;

public sealed class PostgresTableHelper()
{
    public async Task CreateTableAsync<T>() where T : BaseDatabaseModels
    {
        var tableName = typeof(T).Name;
        var createTableQuery = GenerateCreateTableQuery<T>(tableName);

        await using var connection = new NpgsqlConnection(PostgresDatabaseConstants.ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(createTableQuery, connection);
        await command.ExecuteNonQueryAsync();
    }
    
    public async Task DeleteTableAsync<T>() where T : BaseDatabaseModels
    {
        var tableName = typeof(T).Name;
        var deleteTableQuery = $"DROP TABLE {tableName} CASCADE";

        await using var connection = new NpgsqlConnection(PostgresDatabaseConstants.ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(deleteTableQuery, connection);
        await command.ExecuteNonQueryAsync();
    }

    private string GenerateCreateTableQuery<T>(string tableName) where T : BaseDatabaseModels
    {
        var sb = new StringBuilder();
        sb.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName} (");

        var properties = typeof(T).GetProperties();
        
        var propertiesAndMappings = properties.Select(prop => $"    {prop.Name} {MapCSharpTypeToPostgresType(prop.PropertyType)}").ToList();

        sb.AppendLine(string.Join(",\n", propertiesAndMappings));
        sb.AppendLine(");");

        return sb.ToString();
    }

    private string MapCSharpTypeToPostgresType(Type type)
    {
        if (type == typeof(int) || type.IsEnum) return "INTEGER";
        if (type == typeof(string)) return "TEXT";
        if (type == typeof(DateTime)) return "TIMESTAMP";
        if (type == typeof(bool)) return "BOOLEAN";
        if (type == typeof(float)) return "REAL";
        if (type == typeof(double)) return "DOUBLE PRECISION";
        if (type == typeof(decimal)) return "NUMERIC";
        if (type == typeof(Guid)) return "UUID";
        
        throw new NotSupportedException($"C# type {type.Name} is not supported.");
    }
}