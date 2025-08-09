using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using FileHandler.Base;
using System.Diagnostics.CodeAnalysis;

namespace FileHandler.Uploaders
{
    [ExcludeFromCodeCoverage(Justification = 
        "No way to Mock Values Of FileHandler. Will Add Integration tests in the future")]
    public sealed class S3FileHandler : IFileHandlerBase, IDisposable
    {
        private readonly string _bucketName;
        private readonly string _folderName;
        private readonly IAmazonS3 _s3Client;

        public S3FileHandler(string accessKey, string secretKey, string bucketName, string folderName, 
            RegionEndpoint? region = null)
        {
            _bucketName = bucketName;
            _folderName = folderName;
            _s3Client = new AmazonS3Client(accessKey, secretKey, region ?? RegionEndpoint.USEast1);
        }

        public async Task<bool> UploadFile(string filePath)
        {
            try
            {
                var fileTransferUtility = new TransferUtility(_s3Client);
                var fileName = Path.GetFileName(filePath);
                var s3Key = string.IsNullOrEmpty(_folderName) 
                    ? fileName 
                    : $"{_folderName.TrimEnd('/')}/{fileName}";

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    FilePath = filePath,
                    BucketName = _bucketName,
                    Key = s3Key
                };

                await fileTransferUtility.UploadAsync(uploadRequest);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while uploading file {filePath}");
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }

        public async Task<string> DownloadFile(string fileId, string downloadedFileName)
        {
            try
            {
                var s3Key = string.IsNullOrEmpty(_folderName) 
                    ? fileId 
                    : $"{_folderName.TrimEnd('/')}/{fileId}";

                var temporaryDirectory = CreateTemporaryDirectory();
                var downloadPath = Path.Combine(temporaryDirectory, downloadedFileName);

                var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.DownloadAsync(new TransferUtilityDownloadRequest
                {
                    BucketName = _bucketName,
                    Key = s3Key,
                    FilePath = downloadPath
                });

                return downloadPath;
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

        public void Dispose()
        {
            _s3Client?.Dispose();
        }
    }
}