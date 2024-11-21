using System.Data.Common;
using Moq;
using Uploader.Enums;
using Uploader.Models;

namespace Uploader.Tests.Models;

public class CredentialStoreTest
{
    [Test]
    public void AssertThatTheReaderIsSerializedCorrectly()
    {
        var readerMock = new Mock<DbDataReader>();
        
        var guid = Guid.NewGuid().ToString();
        const StorageProviderTypes provider = StorageProviderTypes.GoogleDrive;
        const string credential = "xyz";
        const int usedSize = 0;
        const int maxSize = 1234;
        
        readerMock.Setup(x => x.GetString(It.Is<int>(index => index == 0))).Returns(guid);
        readerMock.Setup(x => x.GetInt32(It.Is<int>(index => index == 1))).Returns((int)provider);
        readerMock.Setup(x => x.GetString(It.Is<int>(index => index == 2))).Returns(credential);
        readerMock.Setup(x => x.GetInt32(It.Is<int>(index => index == 3))).Returns(maxSize);
        readerMock.Setup(x => x.GetInt32(It.Is<int>(index => index == 4))).Returns( usedSize);
        
        var serializedCredentialStore = CredentialStore.Deserialize(readerMock.Object);
        
        Assert.That(serializedCredentialStore, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(serializedCredentialStore.Uuid, Is.EqualTo(guid));
            Assert.That(serializedCredentialStore.Provider, Is.EqualTo(provider));
            Assert.That(serializedCredentialStore.credentialAsJson, Is.EqualTo(credential));
            Assert.That(serializedCredentialStore.UsedSizeInBytes, Is.EqualTo(usedSize));
            Assert.That(serializedCredentialStore.MaxSizeInBytes, Is.EqualTo(maxSize));
        });
    }
}