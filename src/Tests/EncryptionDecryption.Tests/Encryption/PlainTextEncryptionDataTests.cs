using EncryptionDecryption.Encryption;

namespace EncryptionDecryption.Tests.Encryption;

public class PlainTextEncryptionDataTests
{
    [TestCase("", "")]
    [TestCase("test - input", "test - input")]
    [TestCase(null, null)]
    public void AssertThatPlainTextEncryptionReturnsExpectedResult(string plainText, string expectedResult)
    {
        Assert.That(new PlainTextEncryptionData().EncryptData(plainText), Is.EqualTo(expectedResult));
    }
}