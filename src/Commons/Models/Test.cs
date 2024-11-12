using System.Data;
using Commons.Interfaces;
using Npgsql;

namespace Commons.Models;

public class Test(int colA, string colB) : BaseDatabaseModels
{
    public int ColA { get; set; } = colA;
    public string ColB { get; set; } = colB;

    public static Test Deserialize(NpgsqlDataReader reader)
    {
        var colA = reader.GetInt32(0);
        var colB = reader.GetString(1);

        return new Test(colA, colB);
    }
}