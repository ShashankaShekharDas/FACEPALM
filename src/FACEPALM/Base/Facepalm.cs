using System.ComponentModel;
using Commons.Constants;
using Commons.Database;
using Commons.Database.Handlers;
using FACEPALM.Exceptions;
using FACEPALM.Interfaces;
using FACEPALM.Models;
using FACEPALM.Services;
using Uploader.Factory;
using Uploader.Models;

namespace FACEPALM.Base
{
    [Description("Facepalm class ONLY HANDLES CHUNKED AND ENCRYPTED FILES. That is why internal")]
    internal class Facepalm : IFacepalm
    {
        private readonly GenericPostgresDatabaseHelper<CredentialStore> _credentialStoreProvider = new();
        private readonly GenericPostgresDatabaseHelper<ChunkUploaderLocation> _chunkUploaderLocation = new();
        private readonly ILogger _logger;

        public Facepalm(ILogger? logger = null)
        {
            _logger = logger ?? new ConsoleLogger(nameof(Facepalm));
        }

        private static List<string> GetAllFilesInDirectory(string directory)
        {
            return Directory.GetFiles(directory).ToList();
        }
        
        public async Task<Dictionary<string, bool>> UploadFolder(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Directory not found: {path}");
            }

            _logger.LogInformation("Starting upload process for folder: {Path}", path);

            try
            {
                var validProviders = await GetValidProvidersWithRetry();
                if (validProviders.Count == 0)
                {
                    throw new NoValidProviderException("No providers with sufficient space available");
                }

                _logger.LogInformation("Found {Count} valid providers", validProviders.Count);

                var allFilesInDirectory = GetAllFilesInDirectory(path);
                _logger.LogInformation("Found {FileCount} files to upload", allFilesInDirectory.Count);

                var uploadResult = new Dictionary<string, bool>();
                var providerIndex = 0;
                const long estimatedChunkSize = 1_000_000; // Move magic number to constant

                foreach (var file in allFilesInDirectory)
                {
                    try
                    {
                        _logger.LogDebug("Processing file: {FileName}", Path.GetFileName(file));

                        var provider = await FindAvailableProvider(validProviders, providerIndex, estimatedChunkSize);
                        if (provider == null)
                        {
                            _logger.LogError("No available provider found for file: {FileName}", Path.GetFileName(file));
                            uploadResult[file] = false;
                            continue;
                        }

                        var fileInfo = new FileInfo(file);
                        if (!fileInfo.Exists)
                        {
                            _logger.LogWarning("File no longer exists, skipping: {FileName}", file);
                            uploadResult[file] = false;
                            continue;
                        }

                        uploadResult[file] = await UploadFile(file, provider);

                        if (uploadResult[file])
                        {
                            await UpdateProviderUsage(provider, fileInfo.Length);
                            await RecordUploadLocation(file, provider.Uuid);
                            _logger.LogInformation("Successfully uploaded: {FileName}", Path.GetFileName(file));
                        }
                        else
                        {
                            _logger.LogWarning("Failed to upload: {FileName}", Path.GetFileName(file));
                        }

                        providerIndex++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing file: {FileName}", Path.GetFileName(file));
                        uploadResult[file] = false;
                    }
                }

                var successCount = uploadResult.Values.Count(success => success);
                _logger.LogInformation("Upload completed. {SuccessCount}/{TotalCount} files uploaded successfully", 
                    successCount, uploadResult.Count);

                return uploadResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error during folder upload for path: {Path}", path);
                throw;
            }
        }

        // Try each file till failure
        public async Task<bool> UploadFile(string path)
        {
            var validProviders = await _credentialStoreProvider.SearchRows(
                [new WhereClause("maxsizeinbytes - usedsizeinbytes", "5000000", DatabaseOperator.GreaterThanOrEqual)],
                CredentialStore.Deserialize);

            if (validProviders.Count == 0)
            {
                throw new NoValidProviderException("No valid providers found");
            }

            return await UploadFile(path, validProviders.First());
        }

        public async Task<bool> UploadFile(string path, CredentialStore credentialStore) => await UploaderFactory.GetUploader(credentialStore).UploadFile(path);

        private async Task<List<CredentialStore>> GetValidProvidersWithRetry(int maxRetries = 3)
        {
            var retryCount = 0;
            const long minimumSpace = 5_000_000; // Move magic number to constant

            while (retryCount < maxRetries)
            {
                try
                {
                    var providers = await _credentialStoreProvider.SearchRows(
                        [new WhereClause("maxsizeinbytes - usedsizeinbytes", minimumSpace.ToString(), DatabaseOperator.GreaterThanOrEqual)],
                        CredentialStore.Deserialize);

                    return providers;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogWarning("Failed to retrieve providers (attempt {Attempt}/{MaxAttempts}): {Error}", 
                        retryCount, maxRetries, ex.Message);

                    if (retryCount >= maxRetries)
                    {
                        _logger.LogError(ex, "Failed to retrieve providers after {MaxAttempts} attempts", maxRetries);
                        throw;
                    }

                    // Exponential backoff
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
                }
            }

            return [];
        }

        private async Task<CredentialStore?> FindAvailableProvider(List<CredentialStore> providers, int startIndex, long requiredSpace)
        {
            var maxAttempts = providers.Count * 2; // Prevent infinite loops
            var attempts = 0;
            var currentIndex = startIndex;

            while (attempts < maxAttempts)
            {
                var provider = providers[currentIndex % providers.Count];
                
                if (provider.UsedSizeInBytes + requiredSpace <= provider.MaxSizeInBytes)
                {
                    return provider;
                }

                currentIndex++;
                attempts++;
            }

            _logger.LogError("All providers are at capacity. Required space: {RequiredSpace} bytes", requiredSpace);
            return null;
        }

        private async Task UpdateProviderUsage(CredentialStore provider, long additionalUsage)
        {
            try
            {
                provider.UsedSizeInBytes += additionalUsage;
                await _credentialStoreProvider.DeleteRows([new WhereClause("uuid", provider.Uuid, DatabaseOperator.Equal)]);
                await _credentialStoreProvider.InsertData([provider]);
                
                _logger.LogDebug("Updated provider {ProviderId} usage by {AdditionalUsage} bytes", 
                    provider.Uuid, additionalUsage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update provider usage for provider {ProviderId}", provider.Uuid);
                throw;
            }
        }

        private async Task RecordUploadLocation(string filePath, string providerUuid)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                await _chunkUploaderLocation.InsertData([new ChunkUploaderLocation(fileName, providerUuid)]);
                
                _logger.LogDebug("Recorded upload location for file {FileName} to provider {ProviderId}", 
                    fileName, providerUuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record upload location for file {FileName}", Path.GetFileName(filePath));
                throw;
            }
        }
    }
}