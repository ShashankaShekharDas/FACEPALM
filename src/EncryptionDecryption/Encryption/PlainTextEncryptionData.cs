using EncryptionDecryption.Interfaces;

namespace EncryptionDecryption.Encryption;

public sealed class PlainTextEncryptionData : IEncryptData
{
    public string EncryptData(string data) => data;
}