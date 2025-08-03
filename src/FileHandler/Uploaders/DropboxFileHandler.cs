using Dropbox.Api;
using Dropbox.Api.Files;
using FileHandler.Base;
using FileHandler.Models;
using System.Diagnostics.CodeAnalysis;

namespace FileHandler.Uploaders
{
    [ExcludeFromCodeCoverage(Justification =
        "No way to Mock Values Of FileHandler. Will Add Integration tests in the future")]
    public sealed class DropboxFileHandler(DropboxSecret secret) : FileHandlerBase
    {
        private DropboxClient GetClient()
        {
            var httpClient = new HttpClient();
            var config = new DropboxClientConfig("FACEPALM") { HttpClient = httpClient };
            return new DropboxClient(secret.RefreshToken, secret.AppKey, secret.AppSecret, config);
        }

        public override async Task<bool> UploadFile(string filePath)
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

        public override Task<string> DownloadFile(string fileId, string downloadedFileName)
        {
            throw new NotImplementedException();
        }
    }
}