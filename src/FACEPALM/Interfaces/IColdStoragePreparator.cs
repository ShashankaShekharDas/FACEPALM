using Commons.Constants;
using FACEPALM.Enums;

namespace FACEPALM.Interfaces
{
    public interface IColdStoragePreparator
    {
        public Task<string> PrepareFileForStorage(string fileLocation, FileType fileType, EncryptionType encryptionType, int chunkSize, 
            params string[] encryptionParameters);
    }
}