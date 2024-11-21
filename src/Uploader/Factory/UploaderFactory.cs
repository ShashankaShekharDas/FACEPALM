using Uploader.Base;
using Uploader.Enums;
using Uploader.Models;
using Uploader.Uploaders;

namespace Uploader.Factory;

public static class UploaderFactory
{
    public static UploaderBase GetUploader(CredentialStore store)
    {
        return store.Provider switch
        {
            StorageProviderTypes.GoogleDrive => GetGoogleDriveUploader(store),
            _ => GetDropboxUploader(store)
        };
    }

    private static GoogleDriveUploaderBase GetGoogleDriveUploader(CredentialStore store)
    {
        var secret = GoogleDriveSecret.GetDeserializedContent(store.credentialAsJson);
        if (secret is null) throw new ArgumentNullException(nameof(store));
        return new GoogleDriveUploaderBase(GoogleDriveUploaderBase.GenerateStreamFromString(secret.FileContent),
            secret.FolderId);
    }

    private static DropboxUploaderBase GetDropboxUploader(CredentialStore store)
    {
        var secret = DropboxSecret.GetDeserializedContent(store.credentialAsJson);
        if (secret is null) throw new ArgumentNullException(nameof(store));
        return new DropboxUploaderBase(secret);
    }
}