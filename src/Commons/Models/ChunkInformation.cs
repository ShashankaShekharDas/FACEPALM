using Commons.Constants;
using Commons.Interfaces;
using Npgsql;

namespace Commons.Models
{
    public class ChunkInformation(string fileName, string folderName, int totalChunks, EncryptionType encryption, string serializedName)
        : IDatabaseModels
    {
        public string FileName { get; set; } = fileName;
        // When uploading 1 file, the filename itself is the folder name
        public string FolderName { get; set; } = folderName;

        // Serialized Name is the name given to the file while uploading
        // can be encrypted, can be uuid.....
        public string SerializedName { get; set; } = serializedName;
        public int TotalChunks { get; set; } = totalChunks;
        public EncryptionType Encryption { get; set; } = encryption;

        public static IDatabaseModels Deserialize(NpgsqlDataReader reader)
        {
            var fileName = reader.GetString(0);
            var folderName = reader.GetString(1);
            var totalChunks = reader.GetInt32(2);
            var encryption = (EncryptionType)reader.GetInt32(3);
            var serializedName = reader.GetString(4);

            return new ChunkInformation(fileName, folderName, totalChunks, encryption, serializedName);
        }
    }
}