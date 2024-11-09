namespace EncryptionDecryption.Interfaces;

public interface IDecryptData
{
    public string DecryptData(byte[] encodedByteArray);
}