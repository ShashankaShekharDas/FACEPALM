using StartupHandler.FirstTimeStartup;
using StartupHandler.Teardown;

DeleteTables.DeleteTablesInDatabaseIfExists();
CreateTables.CreateTablesInDatabaseIfNotExists();