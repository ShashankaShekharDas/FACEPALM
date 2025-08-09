using Commons.Constants;
using Commons.Interfaces;
using Npgsql;

namespace Commons.Models
{
    public class ChunkInformation(string fileName, string folderName, int totalChunks, EncryptionType encryption, string serializedName, string hash)
        : IDatabaseModels
    {
        public string FileName { get; set; } = fileName;
        public string FolderName { get; set; } = folderName;
        public string SerializedName { get; set; } = serializedName;
        public int TotalChunks { get; set; } = totalChunks;
        public EncryptionType Encryption { get; set; } = encryption;
        public string Hash { get; set; } = hash; // stores hash of encrypted file to avoid decrypting and then having to calculate hash

        public static IDatabaseModels Deserialize(NpgsqlDataReader reader)
        {
            var fileName = reader.GetString(0);
            var folderName = reader.GetString(1);
            var totalChunks = reader.GetInt32(2);
            var encryption = (EncryptionType)reader.GetInt32(3);
            var serializedName = reader.GetString(4);
            var hash = reader.GetString(5);

            return new ChunkInformation(fileName, folderName, totalChunks, encryption, serializedName, hash);
        }
    }
}