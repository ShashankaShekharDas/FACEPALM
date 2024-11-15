using Commons.Interfaces;
using Npgsql;

namespace Commons.Test.Models;

public class TestDatabase(int colA, string colB) : IDatabaseModels
{
    public int ColA { get; } = colA;
    public string ColB { get; } = colB;

    public static TestDatabase Deserialize(NpgsqlDataReader reader)
    {
        var colA = reader.GetInt32(0);
        var colB = reader.GetString(1);

        return new TestDatabase(colA, colB);
    }
}