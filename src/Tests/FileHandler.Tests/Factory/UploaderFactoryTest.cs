using Uploader.Enums;
using Uploader.Factory;
using Uploader.Models;
using Uploader.Uploaders;

namespace Uploader.Tests.Factory
{
    public class UploaderFactoryTest
    {
        private const string GoogleDriveCredential =
            "{\"FileName\" : \"a.b\",\"FileContent\" : \"abcd\",\"FolderId\" : \"xyz\"}";

        private const string DropBoxCredential =
            "{\"RefreshToken\" : \"refresh\",\"AppKey\" : \"key\",\"AppSecret\" : \"secret\",\"Folder\" : \"folder\"}";

        [TestCase(StorageProviderTypes.GoogleDrive, GoogleDriveCredential, typeof(GoogleDriveHandler))]
        [TestCase(StorageProviderTypes.Dropbox, DropBoxCredential, typeof(DropboxFileHandler))]
        public void AssertThatFactoryReturnsCorrectType(StorageProviderTypes provider, string serializedSecret,
            Type expectedType)
        {
            var uploaderObject =
                UploaderFactory.GetUploader(new CredentialStore(Guid.NewGuid().ToString(), provider, serializedSecret,
                    0, 1));
            Assert.That(uploaderObject.GetType(), Is.EqualTo(expectedType));
        }

        [TestCase(StorageProviderTypes.GoogleDrive)]
        [TestCase(StorageProviderTypes.Dropbox)]
        public void AssertThatEmptySecretThrowsArgumentException(StorageProviderTypes provider)
        {
            Assert.That(
                () => UploaderFactory.GetUploader(new CredentialStore(Guid.NewGuid().ToString(), provider, "{}", 0, 1)),
                Throws.ArgumentException);
        }
    }
}