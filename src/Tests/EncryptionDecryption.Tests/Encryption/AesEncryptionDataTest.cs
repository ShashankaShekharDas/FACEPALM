using EncryptionDecryption.Encryption;

namespace EncryptionDecryption.Tests.Encryption;

public class AesEncryptionDataTest
{
    [TestCase("", "")]
    [TestCase("test - input", "test - input")]
    [TestCase(null, null)]
    public void AssertThatAesTextEncryptionReturnsExpectedResult(string plainText, string expectedResult)
    {
        Assert.That(new AesEncryptionData("12345678").EncryptData(plainText), Is.EqualTo(expectedResult));
    }
}