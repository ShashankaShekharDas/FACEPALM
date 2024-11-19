using System.ComponentModel.DataAnnotations.Schema;
using Commons.Interfaces;
using Uploader.Enums;

namespace Uploader.Models;

public class CredentialStore(string uuid, StorageProviderTypes provider, string credential, int maxSize, int usedSize) : IDatabaseModels
{
    public string Uuid { get;} = uuid;
    public StorageProviderTypes Provider { get; } = provider;
    // Must only contain JSON serialized info of Class Secret 
    public string credentialAsJson { get; } = credential;
    public int MaxSizeInBytes { get; } = maxSize;
    public int UsedSizeInBytes { get; } = usedSize;
}