using Commons.Database.Handlers;
using System.Data.Common;
using Commons.Interfaces;
using Uploader.Enums;

namespace Uploader.Models
{
    public class CredentialStore(
        string uuid,
        StorageProviderTypes provider,
        string credential,
        long maxSize,
        long usedSize)
        : IDatabaseModels
    {
        public string Uuid { get; } = uuid;
        public StorageProviderTypes Provider { get; } = provider;
        public string CredentialAsJson { get; } = credential;
        public long MaxSizeInBytes { get; } = maxSize;
        public long UsedSizeInBytes { get; set; } = usedSize;

        public static async Task<bool> UploadSecretToDatabaseAsync(CredentialStore credentialStore)
        {
            var databaseObject = new GenericPostgresDatabaseHelper<CredentialStore>();
            return await databaseObject.InsertData([credentialStore]);
        }

        public static CredentialStore Deserialize(DbDataReader reader)
        {
            var uuid = reader.GetString(0);
            var provider = (StorageProviderTypes)reader.GetInt32(1);
            var credentials = reader.GetString(2);
            var maxSize = reader.GetInt64(3);
            var usedSize = reader.GetInt64(4);

            return new CredentialStore(uuid, provider, credentials, maxSize, usedSize);
        }
    }
}