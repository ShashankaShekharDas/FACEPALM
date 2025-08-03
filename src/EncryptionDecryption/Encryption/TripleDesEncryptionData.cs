using System.Security.Cryptography;
using System.Text;
using EncryptionDecryption.Interfaces;

namespace EncryptionDecryption.Encryption
{
    [Obsolete("Triple Des is not a very secure encryption method and might be removed in the future. Ref S5547")]
    public sealed class TripleDesEncryptionData : IEncryptData
    {
        private readonly TripleDES _tripleDes;

        public TripleDesEncryptionData(string key, string iv)
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

        public byte[] EncryptData(string plainText)
        {
            #region Conditions

            if (plainText is not { Length: > 0 })
            {
                return [];
            }

            #endregion

            #region Encryption

            byte[] encryptedText;
            var encryptor = _tripleDes.CreateEncryptor(_tripleDes.Key, _tripleDes.IV);

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

        public byte[] EncryptData(byte[] plainText) => EncryptData(Convert.ToBase64String(plainText));
    }
}