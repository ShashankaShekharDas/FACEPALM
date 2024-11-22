using Commons.Constants;
using Commons.Database.Handlers;
using EncryptionDecryption.Factory;
using FACEPALM.Enums;
using FACEPALM.Interfaces;
using FACEPALM.Models;

namespace FACEPALM.Base;

public sealed class ColdStoragePreparator : IColdStoragePreparator
{
    private readonly Chunker.Chunker _chunker = new();
    private readonly GenericPostgresDatabaseHelper<FileNameMapping> _fileNameMappingRepo = new();
    public async Task<string> PrepareFileForStorage(string fileLocation, FileType fileType, EncryptionType encryptionType, params string[] encryptionParameters)
    {
        if (fileType == FileType.File)
        {
            var temporaryDirectoryPath = CreateTemporaryDirectory();
            File.Copy(fileLocation, Path.Combine(temporaryDirectoryPath, Path.GetFileName(fileLocation)));

            fileLocation = temporaryDirectoryPath;
        }
        
        var encryptedAndChunkedDirectory = CreateTemporaryDirectory();
        var encryptor = EncryptionFactory.GetEncryptor(encryptionType, encryptionParameters);

        foreach (var file in Directory.GetFiles(fileLocation))
        {
            var fileName = Path.GetFileName(file);
            var serializedFileName = Guid.NewGuid().ToString();
            
            var encryptedBytes = encryptor.EncryptData(File.ReadAllBytes(file));
            var encryptedString = Convert.ToBase64String(encryptedBytes);
            var chunkedString = _chunker.ChunkIncoming(encryptedString).ToList();
            var fileNameMapping = new FileNameMapping(fileName, serializedFileName, chunkedString.Count);
            await _fileNameMappingRepo.InsertData([fileNameMapping]);
            var index = 0;

            foreach (var chunk in chunkedString)
            {
                await File.WriteAllTextAsync(Path.Combine(encryptedAndChunkedDirectory, $"{serializedFileName}-{index++}.shas"), chunk);
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