namespace Commons.Constants;

public static class PostgresDatabaseConstants
{
    public static readonly string ConnectionString = Environment.GetEnvironmentVariable("PostgresProdConnectionString") ?? "CONNECTION-STRING-NOT-FOUND";
    public const string GetSchemaQuery = "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = {tableName}";
    public const string InsertQuery = "INSERT INTO {tableName}({commaSeparatedColumns}) VALUES ({commaSeparatedPlaceHolders})";
    public const string SelectQuery = "SELECT * FROM {tableName} {whereClause}";
    public const string DeleteQuery = "DELETE FROM {tableName} {whereClause}";
}