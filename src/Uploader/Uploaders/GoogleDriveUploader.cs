using System.Diagnostics.CodeAnalysis;
using Commons.Constants;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Uploader.Base;
using File = Google.Apis.Drive.v3.Data.File;

namespace Uploader.Uploaders
{
    [ExcludeFromCodeCoverage(Justification =
        "No way to Mock Values Of Uploader. Will Add Integration tests in the future")]
    public sealed class GoogleDriveUploader(Stream credentialStream, string folderId) : UploaderBase
    {
        public GoogleDriveUploader(string pathToCredentials, string folderId) : this(
            new FileStream(pathToCredentials, FileMode.Open, FileAccess.Read), folderId)
        {
        }

        ~GoogleDriveUploader()
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

        private GoogleCredential GetCredential()
        {
            return GoogleCredential
                .FromStream(credentialStream)
                .CreateScoped(DriveService.ScopeConstants.DriveFile);
        }

        public override async Task<bool> UploadFile(string filePath)
        {
            var googleDriveCredential = GetCredential();

            var driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = googleDriveCredential, ApplicationName = General.ApplicationName
            });

            var fileMetaData = new File { Name = Path.GetFileName(filePath), Parents = new List<string> { folderId } };

            var isUploadSuccess = false;
            await using var streamFile = new FileStream(filePath, FileMode.Open);
            var request = driveService.Files.Create(fileMetaData, streamFile, "");
            request.Fields = "id";
            var uploadStatus = await request.UploadAsync();
            if (uploadStatus != null)
            {
                isUploadSuccess = uploadStatus.Status == UploadStatus.Completed;
            }

            return isUploadSuccess;
        }
    }
}