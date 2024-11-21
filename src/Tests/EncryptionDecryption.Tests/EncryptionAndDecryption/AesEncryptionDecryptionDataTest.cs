using EncryptionDecryption.Decryption;
using EncryptionDecryption.Encryption;

namespace EncryptionDecryption.Tests.EncryptionAndDecryption;

public class AesEncryptionDecryptionDataTest
{
    private const string Key = "12345678901234567890123456789012";
    private const string Iv = "1234567890123456";

    [TestCase("")]
    [TestCase("test - input")]
    public void AssertThatAesTextEncryptionReturnsExpectedResult(string plainText)
    {
        // key needs to be one of 128/256 bits. IV -> 16 bytes
        var encryptedSecret = new AesEncryptionData(Key, Iv).EncryptData(plainText);
        var decryptedSecret = new AesDecryptionData(Key, Iv).DecryptData(encryptedSecret);

        Assert.That(decryptedSecret, Is.EqualTo(plainText));
    }
}