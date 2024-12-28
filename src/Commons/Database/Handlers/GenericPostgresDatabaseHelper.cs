using Commons.Constants;
using Commons.Interfaces;
using Npgsql;

namespace Commons.Database.Handlers
{
    public class GenericPostgresDatabaseHelper<T> : IGenericDatabaseHelper<T>
        where T : IDatabaseModels
    {
        public async Task<bool> InsertData(List<T> rowsToInsert)
        {
            if (rowsToInsert.Count == 0)
            {
                return false;
            }

            var tableName = typeof(T).Name;
            var rowsModified = 0;

            foreach (var row in rowsToInsert)
            {
                var columns = new List<string>();
                var values = new List<string>();
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

                var insertQuery = PostgresDatabaseConstants.InsertQuery
                    .Replace("{tableName}", tableName)
                    .Replace("{commaSeparatedColumns}", string.Join(",", columns))
                    .Replace("{commaSeparatedPlaceHolders}", string.Join(",", values.Select(value => $"'{value}'")));

                await using var dataSource = NpgsqlDataSource.Create(PostgresDatabaseConstants.ConnectionString);
                await using var npgsqlCommand = dataSource.CreateCommand(insertQuery);

                var rowModified = await npgsqlCommand.ExecuteNonQueryAsync();
                rowsModified += rowModified;
            }

            return rowsModified == rowsToInsert.Count;
        }

        public async Task<List<T>> SearchRows(List<WhereClause>? conditions,
            Func<NpgsqlDataReader, T> readerDeserializer)
        {
            var resultList = new List<T>();
            var tableName = typeof(T).Name;
            var selectQuery = PostgresDatabaseConstants.SelectQuery.Replace("{tableName}", tableName)
                .Replace("{whereClause}", WhereClause.GenerateWhereClause(conditions));

            await using var dataSource = NpgsqlDataSource.Create(PostgresDatabaseConstants.ConnectionString);
            await using var npgsqlCommand = dataSource.CreateCommand(selectQuery);
            var reader = await npgsqlCommand.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                resultList.Add(readerDeserializer(reader));
            }

            return resultList;
        }

        public async Task<bool> DeleteRows(List<WhereClause>? conditions)
        {
            var tableName = typeof(T).Name;
            var deleteQuery = PostgresDatabaseConstants.DeleteQuery.Replace("{tableName}", tableName)
                .Replace("{whereClause}", WhereClause.GenerateWhereClause(conditions));
            await using var dataSource = NpgsqlDataSource.Create(PostgresDatabaseConstants.ConnectionString);
            await using var npgsqlCommand = dataSource.CreateCommand(deleteQuery);
            var rowsAffected = await npgsqlCommand.ExecuteNonQueryAsync();

            return rowsAffected >= 1;
        }
    }
}