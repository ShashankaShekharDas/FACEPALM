using System.Buffers;
using Commons.Database.Handlers;
using Commons.Models;
using EncryptionDecryption.Factory;
using FACEPALM.Enums;
using FACEPALM.Interfaces;
using FACEPALM.Models;
using FACEPALM.Services;

namespace FACEPALM.Base
{
    /// <summary>
    /// Memory-efficient version of ColdStoragePreparator that uses streaming for large files
    /// Reduces memory footprint by processing files in chunks instead of loading entirely into memory
    /// </summary>
    public class StreamingColdStoragePreparator : IColdStoragePreparator
    {
        private readonly Chunker.Chunker _chunker = new();
        private readonly GenericPostgresDatabaseHelper<ChunkInformation> _fileChunkInformation = new();
        private readonly ILogger _logger;
        
        // Threshold for switching to streaming mode (100MB)
        private const long StreamingThreshold = 100_000_000;
        private const int BufferSize = 8192; // 8KB buffer for streaming

        public StreamingColdStoragePreparator(ILogger? logger = null)
        {
            _logger = logger ?? new ConsoleLogger(nameof(StreamingColdStoragePreparator));
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

            _logger.LogInformation("Starting streaming file preparation for storage. Path: {Path}, Type: {FileType}", 
                fileLocation, fileType);

            try
            {
                var folderName = Path.GetFileName(fileLocation);
                var workingDirectory = fileLocation;

                // Handle single file case
                if (fileType == FileType.File)
                {
                    workingDirectory = await PrepareSingleFile(fileLocation);
                    folderName = Path.GetFileNameWithoutExtension(fileLocation);
                }

                var encryptedAndChunkedDirectory = CreateTemporaryDirectory();
                var encryptor = EncryptionFactory.GetEncryptor(encryptionType, encryptionParameters);
                var files = Directory.GetFiles(workingDirectory);
                
                _logger.LogInformation("Processing {FileCount} files with streaming approach", files.Length);

                var processedFiles = 0;
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        
                        if (fileInfo.Length > StreamingThreshold)
                        {
                            _logger.LogInformation("Using streaming mode for large file: {FileName} ({FileSize} bytes)", 
                                Path.GetFileName(file), fileInfo.Length);
                            await ProcessLargeFileStreaming(file, folderName, encryptionType, chunkSize, encryptor, encryptedAndChunkedDirectory);
                        }
                        else
                        {
                            await ProcessSmallFile(file, folderName, encryptionType, chunkSize, encryptor, encryptedAndChunkedDirectory);
                        }
                        
                        processedFiles++;
                        
                        if (processedFiles % 5 == 0)
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

                _logger.LogInformation("Streaming file preparation completed successfully. Output: {OutputDir}", encryptedAndChunkedDirectory);
                return encryptedAndChunkedDirectory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error during streaming file preparation: {Path}", fileLocation);
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
                
                // Use async file copy for better performance
                using var sourceStream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true);
                using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, useAsync: true);
                
                await sourceStream.CopyToAsync(destinationStream);
                
                _logger.LogDebug("Copied single file {FileName} to temporary directory using streaming", fileName);
                return temporaryDirectoryPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to prepare single file: {FileName}", Path.GetFileName(fileLocation));
                throw;
            }
        }

        private async Task ProcessSmallFile(string file, string folderName, EncryptionType encryptionType, 
            int chunkSize, dynamic encryptor, string outputDirectory)
        {
            var fileName = Path.GetFileName(file);
            var serializedFileName = Guid.NewGuid().ToString();
            
            _logger.LogDebug("Processing small file: {FileName}", fileName);

            try
            {
                var fileContentAsBytes = await File.ReadAllBytesAsync(file);
                var encryptedBytes = encryptor.EncryptData(fileContentAsBytes);
                var encryptedString = Convert.ToBase64String(encryptedBytes);
                
                var chunkedString = _chunker.ChunkIncoming(encryptedString, chunkSize).ToList();
                
                await SaveChunkInformation(fileName, folderName, encryptionType, serializedFileName, chunkedString.Count);
                await WriteChunksToFiles(chunkedString, serializedFileName, outputDirectory);
                
                _logger.LogDebug("Successfully processed small file: {FileName} into {ChunkCount} chunks", fileName, chunkedString.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing small file: {FileName}", fileName);
                throw;
            }
        }

        private async Task ProcessLargeFileStreaming(string file, string folderName, EncryptionType encryptionType, 
            int chunkSize, dynamic encryptor, string outputDirectory)
        {
            var fileName = Path.GetFileName(file);
            var serializedFileName = Guid.NewGuid().ToString();
            
            _logger.LogDebug("Processing large file with streaming: {FileName}", fileName);

            try
            {
                var chunks = new List<string>();
                var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
                
                try
                {
                    using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true);
                    using var memoryStream = new MemoryStream();
                    
                    int bytesRead;
                    var totalBytesRead = 0L;
                    var fileSize = new FileInfo(file).Length;
                    
                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, BufferSize)) > 0)
                    {
                        await memoryStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                        
                        // Process in chunks to avoid memory buildup
                        if (memoryStream.Length >= chunkSize * 2) // Process when we have enough data
                        {
                            var dataToProcess = memoryStream.ToArray();
                            memoryStream.SetLength(0);
                            memoryStream.Position = 0;
                            
                            await ProcessDataChunk(dataToProcess, encryptor, chunks, chunkSize);
                        }
                        
                        // Log progress for very large files
                        if (totalBytesRead % (50 * 1024 * 1024) == 0) // Every 50MB
                        {
                            var progress = (double)totalBytesRead / fileSize * 100;
                            _logger.LogDebug("Progress for {FileName}: {Progress:F1}%", fileName, progress);
                        }
                    }
                    
                    // Process remaining data
                    if (memoryStream.Length > 0)
                    {
                        var remainingData = memoryStream.ToArray();
                        await ProcessDataChunk(remainingData, encryptor, chunks, chunkSize);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
                
                await SaveChunkInformation(fileName, folderName, encryptionType, serializedFileName, chunks.Count);
                await WriteChunksToFiles(chunks, serializedFileName, outputDirectory);
                
                _logger.LogDebug("Successfully processed large file: {FileName} into {ChunkCount} chunks", fileName, chunks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing large file: {FileName}", fileName);
                throw;
            }
        }

        private async Task ProcessDataChunk(byte[] data, dynamic encryptor, List<string> chunks, int chunkSize)
        {
            await Task.Run(() =>
            {
                var encryptedBytes = encryptor.EncryptData(data);
                var encryptedString = Convert.ToBase64String(encryptedBytes);
                var dataChunks = _chunker.ChunkIncoming(encryptedString, chunkSize);
                chunks.AddRange(dataChunks);
            });
        }

        private async Task SaveChunkInformation(string fileName, string folderName, EncryptionType encryptionType, 
            string serializedFileName, int chunkCount)
        {
            var chunkInformation = new ChunkInformation(fileName, folderName, chunkCount, encryptionType, serializedFileName);
            await _fileChunkInformation.InsertData([chunkInformation]);
        }

        private async Task WriteChunksToFiles(List<string> chunks, string serializedFileName, string outputDirectory)
        {
            var tasks = new List<Task>();
            
            for (var index = 0; index < chunks.Count; index++)
            {
                var chunkIndex = index; // Capture for closure
                var chunk = chunks[chunkIndex];
                var chunkFileName = $"{serializedFileName}-{chunkIndex}.shas";
                var chunkFilePath = Path.Combine(outputDirectory, chunkFileName);
                
                // Write chunks in parallel for better performance
                tasks.Add(File.WriteAllTextAsync(chunkFilePath, chunk));
                
                // Limit concurrent writes to avoid overwhelming the system
                if (tasks.Count >= Environment.ProcessorCount)
                {
                    await Task.WhenAll(tasks);
                    tasks.Clear();
                }
            }
            
            // Wait for remaining tasks
            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
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