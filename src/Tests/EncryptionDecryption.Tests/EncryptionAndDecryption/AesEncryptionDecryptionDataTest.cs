using EncryptionDecryption.Decryption;
using EncryptionDecryption.Encryption;

namespace EncryptionDecryption.Tests.EncryptionAndDecryption;

public class AesEncryptionDecryptionDataTest
{
    private const string key = "12345678901234567890123456789012";
    private const string iv = "1234567890123456";

    [TestCase("")]
    [TestCase("test - input")]
    public void AssertThatAesTextEncryptionReturnsExpectedResult(string plainText)
    {
        // key needs to be one of 128/256 bits. IV -> 16 bytes
        var encryptedSecret = new AesEncryptionData(key, iv).EncryptData(plainText);
        var decryptedSecret = new AesDecryptionData(key, iv).DecryptData(encryptedSecret);

        Assert.That(decryptedSecret, Is.EqualTo(plainText));
    }
}