using System.Security.Cryptography;
using System.Text;
using EncryptionDecryption.Interfaces;

namespace EncryptionDecryption.Encryption;

public sealed class AesEncryptionData : IEncryptData
{
    private readonly Aes _aes;

    public AesEncryptionData(string key)
    {
        _aes = Aes.Create();
        _aes.Key = Encoding.UTF8.GetBytes(key);
    }
    
    public string EncryptData(string plainText)
    {
        #region Conditions

        if (plainText is not { Length: > 0 })
        {
            return plainText;
        }
        if (_aes.Key is not { Length: > 0 })
        {
            throw new ArgumentNullException(nameof(_aes.Key));
        }
        if (_aes.IV is not { Length: > 0 })
        {
            throw new ArgumentNullException(nameof(_aes.IV));
        }
        #endregion
        
        #region Encryption
        var encryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV);
        using var encryptedMemoryStream = new MemoryStream();
        using var encryptedCryptoStream = new CryptoStream(encryptedMemoryStream, encryptor, CryptoStreamMode.Write);
        using var encryptedStreamWriter = new StreamWriter(encryptedCryptoStream);
        encryptedStreamWriter.Write(plainText);
        #endregion
        
        return Encoding.Default.GetString(encryptedMemoryStream.ToArray());
    }
}