using EncryptionDecryption.Interfaces;

namespace EncryptionDecryption.Decryption;

public sealed class PlainTextDecryptionData : IDecryptData
{
    public string EncryptData(string data) => data;
}