using Commons.Constants;
using Commons.Interfaces;
using Npgsql;

namespace Commons.Models
{
    public class ChunkInformation(string fileName, int chunkNumber, int totalChunks, EncryptionType encryption)
        : IDatabaseModels
    {
        public string FileName { get; set; } = fileName;

        public int ChunkNumber { get; set; } = chunkNumber;

        public int TotalChunks { get; set; } = totalChunks;

        public EncryptionType Encryption { get; set; } = encryption;

        public static IDatabaseModels Deserialize(NpgsqlDataReader reader)
        {
            var fileName = reader.GetString(0);
            var chunkNumber = reader.GetInt32(1);
            var totalChunks = reader.GetInt32(2);
            var encryption = (EncryptionType)reader.GetInt32(3);

            return new ChunkInformation(fileName, chunkNumber, totalChunks, encryption);
        }
    }
}