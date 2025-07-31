using FileHandler.Models;

namespace FileHandler.Tests.Models
{
    public class GoogleDriveSecretTest
    {
        private const string GoogleDriveCredential =
            "{\"FileName\":\"a.b\",\"FileContent\":\"abcd\",\"FolderId\":\"xyz\"}";

        private readonly GoogleDriveSecret _googleDriveSecretObject = new("a.b", "abcd", "xyz");

        [TestCase(GoogleDriveCredential)]
        public void AssertThatSecretsAreDeserializedCorrectly(string secret)
        {
            var deserializedObject = GoogleDriveSecret.GetDeserializedContent(secret);
            Assert.Multiple(() =>
            {
                Assert.That(deserializedObject.FileContent, Is.EqualTo("abcd"));
                Assert.That(deserializedObject.FileName, Is.EqualTo("a.b"));
                Assert.That(deserializedObject.FolderId, Is.EqualTo("xyz"));
            });
        }

        [Test]
        public void AssertThatSecretsAreSerializedCorrectly()
        {
            Assert.That(GoogleDriveSecret.GetSerializedContent(_googleDriveSecretObject),
                Is.EqualTo(GoogleDriveCredential));
        }
    }
}