using System.Security.Cryptography;
using System.Text;
using EncryptionDecryption.Interfaces;

namespace EncryptionDecryption.Decryption
{
    public sealed class RsaDecryptionData : IDecryptData
    {
        private readonly RSA _rsa;

        public RsaDecryptionData(string privateKey)
        {
            if (privateKey is not { Length: > 0 })
            {
                throw new ArgumentNullException(nameof(privateKey));
            }

            _rsa = RSA.Create();
            _rsa.FromXmlString(privateKey);
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

            byte[] decryptedData = _rsa.Decrypt(encodedByteArray, RSAEncryptionPadding.OaepSHA256);
            var plainText = Encoding.UTF8.GetString(decryptedData);

            #endregion

            return plainText;
        }
    }
}