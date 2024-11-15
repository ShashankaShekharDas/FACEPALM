using System.Security.Cryptography;
using System.Text;
using EncryptionDecryption.Interfaces;

namespace EncryptionDecryption.Encryption;

public sealed class AesEncryptionData : IEncryptData
{
    private readonly Aes _aes;

    public AesEncryptionData(string key, string iv)
    {
        if (key is not { Length: > 0 }) throw new ArgumentNullException(nameof(key));
        if (iv is not { Length: > 0 }) throw new ArgumentNullException(nameof(iv));
        _aes = Aes.Create();
        _aes.Key = Encoding.UTF8.GetBytes(key);
        _aes.IV = Encoding.UTF8.GetBytes(iv);
    }

    public byte[] EncryptData(string plainText)
    {
        #region Conditions

        if (plainText is not { Length: > 0 }) return [];

        #endregion

        #region Encryption

        byte[] encryptedText;
        var encryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV);

        using (var encryptedMemoryStream = new MemoryStream())
        {
            using (var encryptedCryptoStream =
                   new CryptoStream(encryptedMemoryStream, encryptor, CryptoStreamMode.Write))
            {
                using (var encryptedStreamWriter = new StreamWriter(encryptedCryptoStream))
                {
                    encryptedStreamWriter.Write(plainText);
                }
            }

            encryptedText = encryptedMemoryStream.ToArray();
        }

        #endregion

        return encryptedText;
    }
}