using Commons.Constants;
using Commons.Database;
using Commons.Database.Handlers;
using Commons.Interfaces;
using Commons.Models;

// var ob = new PostgresDatabaseHelper();

// await ob.InsertData("test", new Dictionary<string, string>
// {
//    ["cola"] = "1",
//    ["colb"] = "abcd"
// });


// var result = await ob.SearchRows("test", [new WhereClause("cola", "1")]);



var ob = new GenericPostgresDatabaseHelper<ChunkInformation>();
// await ob.InsertData(
//         [
//             new ChunkInformation("test", 0, 0, EncryptionType.Aes)
//         ]
//     );
var res = await ob.SearchRows(null);
//
Console.WriteLine(res.First());
//
// var ob = new PostgresTableCreator();
// await ob.CreateTableAsync<ChunkInformation>();