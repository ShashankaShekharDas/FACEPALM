using Dropbox.Api;
using Dropbox.Api.Files;
using FileHandler.Base;
using FileHandler.Models;
using System.Diagnostics.CodeAnalysis;

namespace FileHandler.Handlers
{
    [ExcludeFromCodeCoverage(Justification =
        "No way to Mock Values Of FileHandler. Will Add Integration tests in the future")]
    public sealed class DropboxFileHandler(DropboxSecret secret) : IFileHandlerBase
    {
        private DropboxClient GetClient()
        {
            var httpClient = new HttpClient();
            var config = new DropboxClientConfig("FACEPALM") { HttpClient = httpClient };
            return new DropboxClient(secret.RefreshToken, secret.AppKey, secret.AppSecret, config);
        }

        public async Task<bool> UploadFile(string filePath)
        {
            using var client = GetClient();
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            bool succeeded;

            try
            {
                using var stream = new MemoryStream(fileBytes);
                var uploadFileName = $"/{secret.Folder}/{Path.GetFileName(filePath)}";
                await client.Files.UploadAsync(uploadFileName, WriteMode.Overwrite.Instance, body: stream);
                succeeded = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while uploading file {filePath}");
                Console.WriteLine(ex.StackTrace);
                succeeded = false;
            }

            return succeeded;
        }

        public async Task<string> DownloadFile(string fileId, string downloadedFileName)
        {
            using var client = GetClient();
            var memoryStream = new MemoryStream();
    
            try
            {
                var filePath = $"/{secret.Folder}/{fileId}";
                var response = await client.Files.DownloadAsync(filePath);
                await using var responseStream = await response.GetContentAsStreamAsync();
                await responseStream.CopyToAsync(memoryStream);
        
                var temporaryDirectory = Path.Combine(CreateTemporaryDirectory(), downloadedFileName);
                await using var fileStream = new FileStream(temporaryDirectory, FileMode.Create);
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(fileStream);
                return temporaryDirectory;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while downloading file {fileId}");
                Console.WriteLine(ex.StackTrace);
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