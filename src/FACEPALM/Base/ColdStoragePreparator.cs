using Commons.Constants;
using Commons.Database.Handlers;
using Commons.Models;
using EncryptionDecryption.Factory;
using FACEPALM.Enums;
using FACEPALM.Interfaces;
using FACEPALM.Models;
using FACEPALM.Services;

namespace FACEPALM.Base
{
    public class ColdStoragePreparator : IColdStoragePreparator
    {
        private readonly Chunker.Chunker _chunker = new();
        private readonly GenericPostgresDatabaseHelper<ChunkInformation> _fileChunkInformation = new();
        private readonly ILogger _logger;

        public ColdStoragePreparator(ILogger? logger = null)
        {
            _logger = logger ?? new ConsoleLogger(nameof(ColdStoragePreparator));
        }

        public async Task<string> PrepareFileForStorage(string fileLocation, FileType fileType,
            EncryptionType encryptionType, int chunkSize, params string[] encryptionParameters)
        {
            // Input validation
            ArgumentException.ThrowIfNullOrEmpty(fileLocation, nameof(fileLocation));
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize, nameof(chunkSize));
            ArgumentNullException.ThrowIfNull(encryptionParameters, nameof(encryptionParameters));

            if (!File.Exists(fileLocation) && !Directory.Exists(fileLocation))
            {
                throw new FileNotFoundException($"Path not found: {fileLocation}");
            }

            _logger.LogInformation("Starting file preparation for storage. Path: {Path}, Type: {FileType}, Encryption: {EncryptionType}, ChunkSize: {ChunkSize}", 
                fileLocation, fileType, encryptionType, chunkSize);

            try
            {
                var folderName = Path.GetFileName(fileLocation);
                var workingDirectory = fileLocation;

                // Handle single file case
                if (fileType == FileType.File)
                {
                    workingDirectory = await PrepareSingleFile(fileLocation);
                    folderName = Path.GetFileNameWithoutExtension(fileLocation);
                    _logger.LogDebug("Prepared single file in temporary directory: {TempDir}", workingDirectory);
                }

                var encryptedAndChunkedDirectory = CreateTemporaryDirectory();
                _logger.LogDebug("Created output directory: {OutputDir}", encryptedAndChunkedDirectory);

                var encryptor = EncryptionFactory.GetEncryptor(encryptionType, encryptionParameters);
                var files = Directory.GetFiles(workingDirectory);
                
                _logger.LogInformation("Processing {FileCount} files for encryption and chunking", files.Length);

                var processedFiles = 0;
                foreach (var file in files)
                {
                    try
                    {
                        await ProcessSingleFile(file, folderName, encryptionType, chunkSize, encryptor, encryptedAndChunkedDirectory);
                        processedFiles++;
                        
                        if (processedFiles % 10 == 0)
                        {
                            _logger.LogInformation("Processed {ProcessedFiles}/{TotalFiles} files", processedFiles, files.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process file: {FileName}", Path.GetFileName(file));
                        throw;
                    }
                }

                _logger.LogInformation("File preparation completed successfully. Output directory: {OutputDir}", encryptedAndChunkedDirectory);
                return encryptedAndChunkedDirectory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error during file preparation for path: {Path}", fileLocation);
                throw;
            }
        }

        private async Task<string> PrepareSingleFile(string fileLocation)
        {
            try
            {
                var temporaryDirectoryPath = CreateTemporaryDirectory();
                var fileName = Path.GetFileName(fileLocation);
                var destinationPath = Path.Combine(temporaryDirectoryPath, fileName);
                
                await Task.Run(() => File.Copy(fileLocation, destinationPath));
                
                _logger.LogDebug("Copied single file {FileName} to temporary directory", fileName);
                return temporaryDirectoryPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to prepare single file: {FileName}", Path.GetFileName(fileLocation));
                throw;
            }
        }

        private async Task ProcessSingleFile(string file, string folderName, EncryptionType encryptionType, 
            int chunkSize, dynamic encryptor, string outputDirectory)
        {
            var fileName = Path.GetFileName(file);
            var serializedFileName = Guid.NewGuid().ToString();
            
            _logger.LogDebug("Processing file: {FileName} -> {SerializedName}", fileName, serializedFileName);

            try
            {
                // Read file with better memory management for large files
                byte[] fileContentAsBytes;
                var fileInfo = new FileInfo(file);
                
                if (fileInfo.Length > 100_000_000) // 100MB threshold
                {
                    _logger.LogWarning("Large file detected ({FileSize} bytes): {FileName}. Processing may take longer.", 
                        fileInfo.Length, fileName);
                }

                fileContentAsBytes = await File.ReadAllBytesAsync(file);

                var encryptedBytes = encryptor.EncryptData(fileContentAsBytes);
                var encryptedString = Convert.ToBase64String(encryptedBytes);
                var chunkedString = _chunker.ChunkIncoming(encryptedString, chunkSize).ToList();
                
                _logger.LogDebug("File {FileName} encrypted and split into {ChunkCount} chunks", fileName, chunkedString.Count);

                // Store chunk information in database
                var chunkInformation = new ChunkInformation(fileName, folderName, chunkedString.Count, encryptionType, serializedFileName);
                await _fileChunkInformation.InsertData([chunkInformation]);

                // Write chunks to files
                var index = 0;
                foreach (var chunk in chunkedString)
                {
                    var chunkFileName = $"{serializedFileName}-{index++}.shas";
                    var chunkFilePath = Path.Combine(outputDirectory, chunkFileName);
                    await File.WriteAllTextAsync(chunkFilePath, chunk);
                }

                _logger.LogDebug("Successfully processed file: {FileName} into {ChunkCount} chunks", fileName, chunkedString.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {FileName}", fileName);
                throw;
            }
        }

        private static string CreateTemporaryDirectory()
        {
            var temporaryDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(temporaryDirectoryPath);
            return temporaryDirectoryPath;
        }
    }
}