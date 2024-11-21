using System.Data.Common;
using Commons.Interfaces;
using Uploader.Enums;

namespace Uploader.Models;

public class CredentialStore(string uuid, StorageProviderTypes provider, string credential, int maxSize, int usedSize)
    : IDatabaseModels
{
    public string Uuid { get; } = uuid;
    public StorageProviderTypes Provider { get; } = provider;
    public string credentialAsJson { get; } = credential;
    public int MaxSizeInBytes { get; } = maxSize;
    public int UsedSizeInBytes { get; } = usedSize;

    public static CredentialStore Deserialize(DbDataReader reader)
    {
        var uuid = reader.GetString(0);
        var provider = (StorageProviderTypes)reader.GetInt32(1);
        var credentials = reader.GetString(2);
        var maxSize = reader.GetInt32(3);
        var usedSize = reader.GetInt32(4);

        return new CredentialStore(uuid, provider, credentials, maxSize, usedSize);
    }
}