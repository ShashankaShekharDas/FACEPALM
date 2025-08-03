using System.Security.Cryptography;
using System.Text;
using EncryptionDecryption.Interfaces;

namespace EncryptionDecryption.Decryption
{
    [Obsolete("Triple Des is not a very secure encryption method and might be removed in the future. Ref S5547")]
    public sealed class TripleDesDecryptionData : IDecryptData
    {
        private readonly TripleDES _tripleDes;

        public TripleDesDecryptionData(string key, string iv)
        {
            if (key is not { Length: > 0 })
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (iv is not { Length: > 0 })
            {
                throw new ArgumentNullException(nameof(iv));
            }

#pragma warning disable S5547
            _tripleDes = TripleDES.Create();
#pragma warning restore S5547
            _tripleDes.Key = Encoding.UTF8.GetBytes(key);
            _tripleDes.IV = Encoding.UTF8.GetBytes(iv);
        }

        public string DecryptData(byte[] encodedByteArray)
        {
            #region Conditions

            if (encodedByteArray is not { Length: > 0 })
            {
                return string.Empty;
            }

            #endregion

            #region Decryption

            string plainText;
            var decryptor = _tripleDes.CreateDecryptor(_tripleDes.Key, _tripleDes.IV);

            using var decryptedMemoryStream = new MemoryStream(encodedByteArray);
            using var decryptedCryptoStream = new CryptoStream(decryptedMemoryStream, decryptor, CryptoStreamMode.Read);
            using var decryptedStreamReader = new StreamReader(decryptedCryptoStream);
            plainText = decryptedStreamReader.ReadToEnd();

            #endregion

            return plainText;
        }
    }
}