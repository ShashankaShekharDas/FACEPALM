using System.Text;
using System.Security.Cryptography;
using EncryptionDecryption.Interfaces;

namespace EncryptionDecryption.Decryption;

public class AesDecryptionData : IDecryptData
{    
    private readonly Aes _aes;

    public AesDecryptionData(string key, string iv)
    {
        _aes = Aes.Create();
        _aes.Key = Encoding.UTF8.GetBytes(key);
        _aes.IV = Encoding.UTF8.GetBytes(iv);
    }
    
    public string DecryptData(byte[] encodedByteArray)
    {
        # region Conditions
        if (encodedByteArray is not { Length: > 0 })
        {
            return string.Empty;
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
        string plainText;
        using (var decryptedMemoryStream = new MemoryStream(encodedByteArray))
        {
            using (var decryptedCryptoStream =
                   new CryptoStream(decryptedMemoryStream, aesDecryptor, CryptoStreamMode.Read))
            {
                using (var decryptedStreamReader = new StreamReader(decryptedCryptoStream))
                {
                    plainText = decryptedStreamReader.ReadToEnd();
                }
            }
        }
        #endregion

        return plainText;
    }
}