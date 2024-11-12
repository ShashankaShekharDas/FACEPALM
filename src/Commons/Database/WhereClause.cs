namespace Commons.Database;

public sealed class WhereClause(string columnName, string value)
{
    public string ColumnName { get; set; } = columnName;
    public string Value { get; set; } = value;
}