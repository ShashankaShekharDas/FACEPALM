using Commons.Database;

namespace Commons.Interfaces;

public interface IGenericDatabaseHelper<T> where T : BaseDatabaseModels
{
    public Task<bool> InsertData(List<T> rowsToInsert);
    public Task<List<T>> SearchRows(List<WhereClause>? conditions);
}