using System.Security.Cryptography;
using System.Text;
using FACEPALM.Configuration;
using FACEPALM.Exceptions;

namespace FACEPALM.Services
{
    public interface IEncryptionKeyManager
    {
        string[] GetEncryptionParameters();
        void ValidateEncryptionEnvironment();
    }

    public class EncryptionKeyManager : IEncryptionKeyManager
    {
        private readonly EncryptionSettings _encryptionSettings;

        public EncryptionKeyManager(EncryptionSettings encryptionSettings)
        {
            _encryptionSettings = encryptionSettings ?? throw new ArgumentNullException(nameof(encryptionSettings));
        }

        public string[] GetEncryptionParameters()
        {
            ValidateEncryptionEnvironment();

            var key = Environment.GetEnvironmentVariable(_encryptionSettings.KeyEnvironmentVariable);
            var iv = Environment.GetEnvironmentVariable(_encryptionSettings.IvEnvironmentVariable);

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(iv))
            {
                throw new SecurityException("Encryption key or IV environment variables are not set properly");
            }

            return [key, iv];
        }

        public void ValidateEncryptionEnvironment()
        {
            var key = Environment.GetEnvironmentVariable(_encryptionSettings.KeyEnvironmentVariable);
            var iv = Environment.GetEnvironmentVariable(_encryptionSettings.IvEnvironmentVariable);

            if (string.IsNullOrEmpty(key))
            {
                throw new SecurityException($"Environment variable '{_encryptionSettings.KeyEnvironmentVariable}' is not set. Please set a secure encryption key.");
            }

            if (string.IsNullOrEmpty(iv))
            {
                throw new SecurityException($"Environment variable '{_encryptionSettings.IvEnvironmentVariable}' is not set. Please set a secure initialization vector.");
            }

            // Validate key length (for AES-256, key should be 32 bytes when base64 decoded)
            try
            {
                var keyBytes = Convert.FromBase64String(key);
                if (keyBytes.Length < 32)
                {
                    throw new SecurityException("Encryption key is too short. Use at least 256-bit (32 bytes) key for AES-256.");
                }
            }
            catch (FormatException)
            {
                throw new SecurityException("Encryption key is not a valid base64 string.");
            }

            // Validate IV length (for AES, IV should be 16 bytes when base64 decoded)
            try
            {
                var ivBytes = Convert.FromBase64String(iv);
                if (ivBytes.Length != 16)
                {
                    throw new SecurityException("Initialization vector must be exactly 128 bits (16 bytes) for AES.");
                }
            }
            catch (FormatException)
            {
                throw new SecurityException("Initialization vector is not a valid base64 string.");
            }
        }

        /// <summary>
        /// Generates a secure random encryption key and IV for initial setup
        /// This method should only be used for initial key generation, not in production code
        /// </summary>
        public static (string Key, string IV) GenerateSecureKeyPair()
        {
            using var aes = Aes.Create();
            aes.KeySize = 256; // AES-256
            aes.GenerateKey();
            aes.GenerateIV();

            return (Convert.ToBase64String(aes.Key), Convert.ToBase64String(aes.IV));
        }
    }
}