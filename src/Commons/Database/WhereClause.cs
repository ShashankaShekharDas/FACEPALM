namespace Commons.Database;

public sealed class WhereClause(string? columnName, string? value)
{
    private string? ColumnName { get; } = columnName;
    private string? Value { get; } = value;

    public static string GenerateWhereClause(List<WhereClause>? conditions)
    {
        var combinedWhereClause = new List<string>();

        if (conditions != null)
            combinedWhereClause.AddRange(conditions
                .Where(clause => !string.IsNullOrEmpty(clause.ColumnName))
                .Select(whereClause => whereClause.Value is null
                    ? $"{whereClause.ColumnName} is null"
                    : $"{whereClause.ColumnName} = '{whereClause.Value}'"));

        var whereClause = "";

        if (combinedWhereClause.Any()) whereClause = $"WHERE {string.Join("AND", combinedWhereClause)}";

        return whereClause;
    }
}