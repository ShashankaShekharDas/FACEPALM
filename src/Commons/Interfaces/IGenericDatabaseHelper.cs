using Commons.Database;
using Npgsql;

namespace Commons.Interfaces
{
    public interface IGenericDatabaseHelper<T> where T : IDatabaseModels
    {
        public Task<bool> InsertData(List<T> rowsToInsert);
        public Task<List<T>> SearchRows(List<WhereClause>? conditions, Func<NpgsqlDataReader, T> readerDeserializer);
        public Task<bool> DeleteRows(List<WhereClause>? conditions);
    }
}