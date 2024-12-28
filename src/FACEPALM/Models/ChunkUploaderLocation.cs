using Commons.Interfaces;
using Npgsql;
using Uploader.Enums;

namespace FACEPALM.Models
{
    public class ChunkUploaderLocation(string serializedName, int chunkIndex, StorageProviderTypes providerType)
        : IDatabaseModels
    {
        public string SerializedName { get; set; } = serializedName;
        public int ChunkIndex { get; set; } = chunkIndex;
        public StorageProviderTypes ProviderType { get; set; } = providerType;

        public static ChunkUploaderLocation Deserialize(NpgsqlDataReader reader)
        {
            var serializedName = reader.GetString(0);
            var chunkIndex = reader.GetInt32(1);
            var providerType = (StorageProviderTypes)reader.GetInt32(2);

            return new ChunkUploaderLocation(serializedName, chunkIndex, providerType);
        }
    }
}