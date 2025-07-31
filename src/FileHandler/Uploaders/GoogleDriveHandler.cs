using System.Diagnostics.CodeAnalysis;
using Commons.Constants;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using FileHandler.Base;
using File = Google.Apis.Drive.v3.Data.File;

namespace FileHandler.Uploaders
{
    [ExcludeFromCodeCoverage(Justification =
        "No way to Mock Values Of FileHandler. Will Add Integration tests in the future")]
    public sealed class GoogleDriveHandler(Stream credentialStream, string folderId) : FileHandlerBase
    {
        public GoogleDriveHandler(string pathToCredentials, string folderId) : this(
            new FileStream(pathToCredentials, FileMode.Open, FileAccess.Read), folderId)
        {
        }

        ~GoogleDriveHandler()
        {
            credentialStream.Dispose();
        }

        public static Stream GenerateStreamFromString(string input)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(input);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private DriveService GetDriveService()
        {
            var googleCredential = GoogleCredential
                .FromStream(credentialStream)
                .CreateScoped(DriveService.ScopeConstants.Drive);
            
            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = googleCredential, ApplicationName = General.ApplicationName
            });
        }

        public override async Task<bool> UploadFile(string filePath)
        {
            var driveService = GetDriveService();
            var fileMetaData = new File { Name = Path.GetFileName(filePath), Parents = new List<string> { folderId }};
            var isUploadSuccess = false;
            await using var streamFile = new FileStream(filePath, FileMode.Open);
            var request = driveService.Files.Create(fileMetaData, streamFile, "");
            request.Fields = "id";
            var uploadStatus = await request.UploadAsync();
            if (uploadStatus != null)
            {
                isUploadSuccess = uploadStatus.Status == UploadStatus.Completed;
            }

            Console.WriteLine(request.ResponseBody.Id);

            return isUploadSuccess;
        }

        public override async Task<string> DownloadFile(string fileId, string downloadedFileName)
        {           
            var driveService = GetDriveService();
            var request = driveService.Files.Get(fileId);
            var memoryStream = new MemoryStream();
            
            request.MediaDownloader.ProgressChanged += progress =>{
                switch (progress.Status)
                {
                    case DownloadStatus.Downloading:
                        {
                            Console.WriteLine(progress.BytesDownloaded);
                            break;
                        }
                    case DownloadStatus.Completed:
                        {
                            Console.WriteLine("Download complete.");
                            break;
                        }
                    case DownloadStatus.Failed:
                        {
                            Console.WriteLine("Download failed.");
                            break;
                        }
                    case DownloadStatus.NotStarted:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
            
            await request.DownloadAsync(memoryStream);
            var temporaryDirectory = Path.Combine(CreateTemporaryDirectory(), downloadedFileName);
            await using var fileStream = new FileStream(temporaryDirectory, FileMode.Create);
            await memoryStream.CopyToAsync(fileStream);
            return temporaryDirectory;
        }
        
        private static string CreateTemporaryDirectory()
    {
            var temporaryDirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(temporaryDirectoryPath);
            return temporaryDirectoryPath;
        }
    }
}