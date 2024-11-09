using EncryptionDecryption.Decryption;
using EncryptionDecryption.Encryption;

namespace EncryptionDecryption.Tests.EncryptionAndDecryption;

public class PlainTextEncryptionDecryptionDataTests
{
    [TestCase("")]
    [TestCase("test - input")]
    public void AssertThatPlainTextEncryptionReturnsExpectedResult(string plainText)
    {
        var encrypted = new PlainTextEncryptionData().EncryptData(plainText);
        var decrypted = new PlainTextDecryptionData().DecryptData(encrypted);
        Assert.That(decrypted, Is.EqualTo(plainText));
    }
}