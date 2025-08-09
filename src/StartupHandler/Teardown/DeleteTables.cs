using Commons.Database.Handlers;
using Commons.Models;
using FACEPALM.Models;
using FileHandler.Models;
using System.Reflection;

namespace StartupHandler.Teardown;

public static class DeleteTables
{
    private static List<Type> GetDatabaseModels() =>
    [
        typeof(ChunkInformation),
        typeof(ChunkUploaderLocation),
        typeof(CredentialStore)
    ];
    
    public static void DeleteTablesInDatabaseIfExists()
    {
        foreach (var tableHelperObject in GetDatabaseModels()
                     .Select(databaseModel => typeof(PostgresTableHelper<>).MakeGenericType(databaseModel)))
        {
            var methodRunner =
                tableHelperObject.GetMethod("DeleteTableAsync", BindingFlags.Public | BindingFlags.Static);

            //Workaround as couldn't use await :)
            ((Task)methodRunner?.Invoke(null, null)!).Wait();
        }
    }
}