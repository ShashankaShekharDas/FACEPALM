using System.Security.Cryptography;
using EncryptionDecryption.Decryption;
using EncryptionDecryption.Encryption;

namespace EncryptionDecryption.Tests.EncryptionAndDecryption
{
    public class RsaEncryptionDecryptionDataTest
    {
        private string _publicKey;
        private string _privateKey;

        [SetUp]
        public void Setup()
        {
            using (var rsa = RSA.Create())
            {
                _privateKey = rsa.ToXmlString(true);  // Include private key
                _publicKey = rsa.ToXmlString(false);  // Public key only
            }
        }

        [TestCase("")]
        [TestCase("test - input")]
        public void AssertThatRsaTextEncryptionReturnsExpectedResult(string plainText)
        {
            var encryptedSecret = new RsaEncryptionData(_publicKey).EncryptData(plainText);
            var decryptedSecret = new RsaDecryptionData(_privateKey).DecryptData(encryptedSecret);

            Assert.That(decryptedSecret, Is.EqualTo(plainText));
        }
    }
}