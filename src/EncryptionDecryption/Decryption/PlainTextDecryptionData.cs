using System.Text;
using EncryptionDecryption.Interfaces;

namespace EncryptionDecryption.Decryption;

public sealed class PlainTextDecryptionData : IDecryptData
{
    public string DecryptData(byte[] encodedByteArray) => Encoding.UTF32.GetString(encodedByteArray);
}