namespace EncryptionDecryption.Interfaces
{
    public interface IEncryptData
    {
        public byte[] EncryptData(string plainText);
        public byte[] EncryptData(byte[] plainText);
    }
}