using Commons.Database.Handlers;
using Commons.Interfaces;
using Commons.Models;
using FACEPALM.Models;
using System.Reflection;
using FileHandler.Models;

namespace StartupHandler.FirstTimeStartup
{
    public static class CreateTables
    {
        //MUST BE UPDATED. WRITE A TEST TO ENFORCE THIS
        private static List<Type> GetDatabaseModels() =>
            [
                typeof(ChunkInformation),
                typeof(ChunkUploaderLocation),
                typeof(CredentialStore)
            ];

        public static void CreateTablesInDatabaseIfNotExists()
        {
            foreach (var tableHelperObject in GetDatabaseModels()
                         .Select(databaseModel => typeof(PostgresTableHelper<>).MakeGenericType(databaseModel)))
            {
                var methodRunner =
                    tableHelperObject.GetMethod("CreateTableAsync", BindingFlags.Public | BindingFlags.Static);

                //Workaround as couldn't use await :)
                ((Task)methodRunner?.Invoke(null, null)!).Wait();
            }
        }
    }
}