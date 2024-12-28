using Commons.Constants;
using EncryptionDecryption.Encryption;
using EncryptionDecryption.Interfaces;

namespace EncryptionDecryption.Factory
{
    public static class EncryptionFactory
    {
        // Bad way for now params. Move to a class
        public static IEncryptData GetEncryptor(EncryptionType encryptionType, params string[] encryptInfo)
        {
            return encryptionType switch
            {
                EncryptionType.Plaintext => new PlainTextEncryptionData(),
                EncryptionType.Aes => new AesEncryptionData(encryptInfo[0], encryptInfo[1]),
                _ => throw new ArgumentException($"Unsupported encryption type: {encryptionType}")
            };
        }
    }
}