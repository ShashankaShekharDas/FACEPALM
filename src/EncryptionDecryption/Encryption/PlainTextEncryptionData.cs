using System.Text;
using EncryptionDecryption.Interfaces;

namespace EncryptionDecryption.Encryption;

public sealed class PlainTextEncryptionData : IEncryptData
{
    public byte[] EncryptData(string plainText)
    {
        return Encoding.UTF32.GetBytes(plainText);
    }

    public byte[] EncryptData(byte[] plainText) => EncryptData(Convert.ToBase64String(plainText));
}