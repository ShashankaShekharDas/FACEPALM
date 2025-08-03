using System.Security.Cryptography;
using System.Text;
using EncryptionDecryption.Interfaces;

namespace EncryptionDecryption.Encryption
{
    public sealed class RsaEncryptionData : IEncryptData
    {
        private readonly RSA _rsa;

        public RsaEncryptionData(string publicKey)
        {
            if (publicKey is not { Length: > 0 })
            {
                throw new ArgumentNullException(nameof(publicKey));
            }

            _rsa = RSA.Create();
            _rsa.FromXmlString(publicKey);
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

            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedData = _rsa.Encrypt(dataToEncrypt, RSAEncryptionPadding.OaepSHA256);

            #endregion

            return encryptedData;
        }

        public byte[] EncryptData(byte[] plainText) => EncryptData(Convert.ToBase64String(plainText));
    }
}