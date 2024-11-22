using Commons.Constants;

namespace Commons.Database;

public sealed class WhereClause(string? columnName, string? value, DatabaseOperator dbOperator)
{
    private string? ColumnName { get; } = columnName; //Null values handled inside generate function
    private string? Value { get; } = value;
    private DatabaseOperator Operator { get; } = dbOperator;

    public static string GenerateWhereClause(List<WhereClause>? conditions)
    {
        var combinedWhereClause = new List<string>();

        if (conditions != null)
            combinedWhereClause.AddRange(conditions
                .Where(clause => !string.IsNullOrEmpty(clause.ColumnName))
                .Select(clause => $"{clause.ColumnName} {GetOperator(clause.Operator)} {GetValue(clause.Value)}"));

        var whereClause = "";

        if (combinedWhereClause.Any()) whereClause = $"WHERE {string.Join("AND", combinedWhereClause)}";

        return whereClause;
    }

    private static string GetValue(string? clauseValue) => string.IsNullOrEmpty(clauseValue) ? "null" : $"'{clauseValue}'";

    private static string GetOperator(DatabaseOperator dbOperator)
    {
        return dbOperator switch
        {
            DatabaseOperator.Equal => "=",
            DatabaseOperator.NotEqual => "!=",
            DatabaseOperator.GreaterThan => ">",
            DatabaseOperator.GreaterThanOrEqual => ">=",
            DatabaseOperator.LessThan => "<",
            DatabaseOperator.LessThanOrEqual => "<=",
            DatabaseOperator.Null => "is", // bad way but query op is x is null,
            _ => "="
        };
    }
}