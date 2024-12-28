using Commons.Database.Handlers;
using Commons.Interfaces;
using System.Reflection;

namespace StartupHandler.FirstTimeStartup;

public static class CreateTables
{
    public static void CreateTablesInDatabaseIfNotExists()
    {
        var databaseModelType = typeof(IDatabaseModels);

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            var typesImplementingInterface = assembly.GetTypes()
                .Where(t => databaseModelType.IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
                .ToList();

            foreach (var executeMethod in typesImplementingInterface.Select(type => typeof(PostgresTableHelper<>).MakeGenericType(type)).Select(genericType => genericType.GetMethod("CreateTableAsync", BindingFlags.Public | BindingFlags.Static)))
            {
                executeMethod?.Invoke(null, null);
            }
        }
    }
}