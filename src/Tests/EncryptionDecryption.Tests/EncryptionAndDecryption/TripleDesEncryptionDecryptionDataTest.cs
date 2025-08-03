using EncryptionDecryption.Decryption;
using EncryptionDecryption.Encryption;

namespace EncryptionDecryption.Tests.EncryptionAndDecryption
{
    public class TripleDesEncryptionDecryptionDataTest
    {
        private const string Key = "123456789012345678901234"; // 24 bytes for Triple DES
        private const string Iv = "12345678"; // 8 bytes for Triple DES

        [TestCase("")]
        [TestCase("test - input")]
        public void AssertThatTripleDesTextEncryptionReturnsExpectedResult(string plainText)
        {
            var encryptedSecret = new TripleDesEncryptionData(Key, Iv).EncryptData(plainText);
            var decryptedSecret = new TripleDesDecryptionData(Key, Iv).DecryptData(encryptedSecret);

            Assert.That(decryptedSecret, Is.EqualTo(plainText));
        }
    }
}