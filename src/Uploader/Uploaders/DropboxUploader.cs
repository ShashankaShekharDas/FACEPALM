using System.Diagnostics.CodeAnalysis;
using Dropbox.Api;
using Dropbox.Api.Files;
using Uploader.Base;
using Uploader.Models;

namespace Uploader.Uploaders;

[ExcludeFromCodeCoverage(Justification = "No way to Mock Values Of Uploader. Will Add Integration tests in the future")]
public sealed class DropboxUploader(DropboxSecret secret) : UploaderBase
{
    private DropboxClient GetClient()
    {
        var httpClient = new HttpClient();
        var config = new DropboxClientConfig("FACEPALM")
        {
            HttpClient = httpClient
        };
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
}