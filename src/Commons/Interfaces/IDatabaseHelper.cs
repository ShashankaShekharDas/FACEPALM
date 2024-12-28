using Commons.Database;

namespace Commons.Interfaces
{
    //Deleting is not allowed for now
    public interface IDatabaseHelper
    {
        public Task<bool> InsertData(string tableName, Dictionary<string, string> columnsAndValues);
        public Task<bool> InsertData(string tableName, string[] columnNames, string[] values);
        public Task<List<string>> SearchRows(string tableName, List<WhereClause>? conditions);
        public Task<bool> DeleteRows(string tableName, List<WhereClause>? conditions);
    }
}