using System.Text;
using EncryptionDecryption.Interfaces;

namespace EncryptionDecryption.Encryption;

public sealed class PlainTextEncryptionData : IEncryptData
{
    public byte[] EncryptData(string data) => Encoding.UTF32.GetBytes(data);
}