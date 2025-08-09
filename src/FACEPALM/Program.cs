using Commons.Constants;
using Commons.Hashers;
using FACEPALM.Base;
using FACEPALM.Enums;

namespace FACEPALM
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            /*
             * Workflow
             * Get File
             * Encrypt
             * Chunk
             * Filename : <filename> - <chunk number> - <total chunks> . shas
             * Store
             * Metadata of where each file is stored
             */

            var coldStoragePreparator = new ColdStoragePreparator(new Sha256Base64Hasher());
            var facepalmObject = new Facepalm();
            
            var temporaryDirectory = await coldStoragePreparator.PrepareFileForStorage("/home/shashanka/Documents/STEAL-Upload/MKBHDWallpapers",
                FileType.Folder, EncryptionType.Aes, 1000000, "12345678901234567890123456789012", "1234567890123456");
            await facepalmObject.UploadFolder(temporaryDirectory);

        }
    }
}