using Uploader.Base;
using Uploader.Enums;
using Uploader.Models;
using Uploader.Uploaders;

namespace Uploader.Factory
{
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

        private static GoogleDriveUploader GetGoogleDriveUploader(CredentialStore store)
        {
            var secret = GoogleDriveSecret.GetDeserializedContent(store.CredentialAsJson);
            if (secret is null)
            {
                throw new ArgumentNullException(nameof(store));
            }

            return new GoogleDriveUploader(GoogleDriveUploader.GenerateStreamFromString(secret.FileContent),
                secret.FolderId);
        }

        private static DropboxUploader GetDropboxUploader(CredentialStore store)
        {
            var secret = DropboxSecret.GetDeserializedContent(store.CredentialAsJson);
            if (secret is null)
            {
                throw new ArgumentNullException(nameof(store));
            }

            return new DropboxUploader(secret);
        }
    }
}