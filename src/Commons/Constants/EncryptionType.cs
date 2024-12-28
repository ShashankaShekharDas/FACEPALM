using System.ComponentModel;

namespace Commons.Constants
{
    public enum EncryptionType
    {
        [DefaultValue(0)] Plaintext = 0,
        [DefaultValue(1)] Aes = 1
    }
}