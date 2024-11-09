using System.Text;
using System.Security.Cryptography;
using EncryptionDecryption.Interfaces;

namespace EncryptionDecryption.Decryption;

public class AesDecryptionData : IDecryptData
{    
    private readonly Aes _aes;

    public AesDecryptionData(string key)
    {
        _aes = Aes.Create();
        _aes.Key = Encoding.UTF8.GetBytes(key);
    }
    
    public string EncryptData(string encodedText)
    {
        # region Conditions
        if (encodedText is not { Length: > 0 })
        {
            return encodedText;
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

        #region Decrypt
        var aesDecryptor = _aes.CreateDecryptor(_aes.Key, _aes.IV);
        using var decryptedMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(encodedText));
        using var decryptedCryptoStream = new CryptoStream(decryptedMemoryStream, aesDecryptor, CryptoStreamMode.Read);
        using var decryptedStreamReader = new StreamReader(decryptedCryptoStream);
        #endregion
        
        return decryptedStreamReader.ReadToEnd();
    }
}