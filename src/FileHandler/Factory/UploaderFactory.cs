using FileHandler.Base;
using FileHandler.Enums;
using FileHandler.Models;
using FileHandler.Uploaders;

namespace FileHandler.Factory
{
    public static class UploaderFactory
    {
        public static FileHandlerBase GetUploader(CredentialStore store)
        {
            return store.Provider switch
            {
                StorageProviderTypes.GoogleDrive => GetGoogleDriveUploader(store),
                _ => GetDropboxUploader(store)
            };
        }

        private static GoogleDriveHandler GetGoogleDriveUploader(CredentialStore store)
        {
            var secret = GoogleDriveSecret.GetDeserializedContent(store.CredentialAsJson);
            if (secret is null)
            {
                throw new ArgumentNullException(nameof(store));
            }

            return new GoogleDriveHandler(GoogleDriveHandler.GenerateStreamFromString(secret.FileContent),
                secret.FolderId);
        }

        private static DropboxFileHandler GetDropboxUploader(CredentialStore store)
        {
            var secret = DropboxSecret.GetDeserializedContent(store.CredentialAsJson);
            if (secret is null)
            {
                throw new ArgumentNullException(nameof(store));
            }

            return new DropboxFileHandler(secret);
        }
    }
}