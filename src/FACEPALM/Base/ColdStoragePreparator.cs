using Commons.Constants;
using Commons.Database.Handlers;
using Commons.Models;
using EncryptionDecryption.Factory;
using FACEPALM.Enums;
using FACEPALM.Interfaces;

namespace FACEPALM.Base
{
    public sealed class ColdStoragePreparator : IColdStoragePreparator
    {
        private readonly Chunker.Chunker _chunker = new();
        private readonly GenericPostgresDatabaseHelper<ChunkInformation> _fileChunkInformation = new();

        public async Task<string> PrepareFileForStorage(string fileLocation, FileType fileType,
            EncryptionType encryptionType, int chunkSize, params string[] encryptionParameters)
        {
            var folderName = Path.GetFileName(fileLocation);
            if (fileType == FileType.File)
            {
                var temporaryDirectoryPath = CreateTemporaryDirectory();
                File.Copy(fileLocation, Path.Combine(temporaryDirectoryPath, Path.GetFileName(fileLocation)));
                fileLocation = temporaryDirectoryPath;
                folderName = Path.GetFileNameWithoutExtension(fileLocation);
            }

            var encryptedAndChunkedDirectory = CreateTemporaryDirectory();
            var encryptor = EncryptionFactory.GetEncryptor(encryptionType, encryptionParameters);

            foreach (var file in Directory.GetFiles(fileLocation))
            {
                var fileName = Path.GetFileName(file);
                var serializedFileName = Guid.NewGuid().ToString();
                var fileContentAsBytes = await File.ReadAllBytesAsync(file);

                var encryptedBytes = encryptor.EncryptData(fileContentAsBytes);
                var encryptedString = Convert.ToBase64String(encryptedBytes);
                var chunkedString = _chunker.ChunkIncoming(encryptedString, chunkSize).ToList();
                var chunkInformation = new ChunkInformation(fileName, folderName, chunkedString.Count, encryptionType, serializedFileName);
                await _fileChunkInformation.InsertData([chunkInformation]);
                var index = 0;

                foreach (var chunk in chunkedString)
                {
                    await File.WriteAllTextAsync(
                        Path.Combine(encryptedAndChunkedDirectory, $"{serializedFileName}-{index++}.shas"), chunk);
                }
            }

            return encryptedAndChunkedDirectory;
        }

        private static string CreateTemporaryDirectory()
        {
            var temporaryDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(temporaryDirectoryPath);
            return temporaryDirectoryPath;
        }
    }
}