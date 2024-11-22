using System.Text;
using Commons.Constants;
using Commons.Interfaces;
using Npgsql;

namespace Commons.Database.Handlers;

public static class PostgresTableHelper
{
    public static async Task CreateTableAsync<T>() where T : IDatabaseModels
    {
        var tableName = typeof(T).Name;
        var createTableQuery = GenerateCreateTableQuery<T>(tableName);

        await using var connection = new NpgsqlConnection(PostgresDatabaseConstants.ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(createTableQuery, connection);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task DeleteTableAsync<T>() where T : IDatabaseModels
    {
        var tableName = typeof(T).Name;
        var deleteTableQuery = $"DROP TABLE {tableName} CASCADE";

        await using var connection = new NpgsqlConnection(PostgresDatabaseConstants.ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(deleteTableQuery, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static string GenerateCreateTableQuery<T>(string tableName) where T : IDatabaseModels
    {
        var sb = new StringBuilder();
        sb.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName} (");

        var properties = typeof(T).GetProperties();

        var propertiesAndMappings = properties
            .Select(prop => $"    {prop.Name} {MapCSharpTypeToPostgresType(prop.PropertyType)}").ToList();

        sb.AppendLine(string.Join(",\n", propertiesAndMappings));
        sb.AppendLine(");");

        return sb.ToString();
    }

    private static string MapCSharpTypeToPostgresType(Type type)
    {
        if (type == typeof(int) || type.IsEnum) return "INTEGER";
        if (type == typeof(string)) return "TEXT";
        if (type == typeof(long)) return "NUMERIC";
        throw new NotSupportedException($"C# type {type.Name} is not supported.");
    }
}