using FileHandler.Models;

namespace FileHandler.Tests.Models
{
    public class DropboxSecretTest
    {
        private const string SerializedDropBoxCredential =
            "{\"RefreshToken\":\"refresh\",\"AppKey\":\"key\",\"AppSecret\":\"secret\",\"Folder\":\"folder\"}";

        private readonly DropboxSecret _dropboxSecretObject = new("refresh", "key", "secret", "folder");

        [TestCase(SerializedDropBoxCredential)]
        public void AssertThatSecretsAreDeserializedCorrectly(string secret)
        {
            var deserializedObject = DropboxSecret.GetDeserializedContent(secret);
            Assert.Multiple(() =>
            {
                Assert.That(deserializedObject.Folder, Is.EqualTo("folder"));
                Assert.That(deserializedObject.RefreshToken, Is.EqualTo("refresh"));
                Assert.That(deserializedObject.AppKey, Is.EqualTo("key"));
                Assert.That(deserializedObject.AppSecret, Is.EqualTo("secret"));
            });
        }

        [Test]
        public void AssertThatSecretsAreSerializedCorrectly()
        {
            Assert.That(DropboxSecret.GetSerializedContent(_dropboxSecretObject),
                Is.EqualTo(SerializedDropBoxCredential));
        }
    }
}