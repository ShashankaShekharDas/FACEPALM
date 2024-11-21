using System.Runtime.InteropServices;
using System.Text;
using Dropbox.Api;
using Dropbox.Api.Files;
using Uploader.Interfaces;
using Uploader.Models;

namespace Uploader.Uploaders;

public sealed class DropboxUploader(DropboxSecret secret) : IUploader
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
    
    public async Task<bool> UploadFile(string filePath)
    {
        using var client = GetClient();
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var succeeded = false;

        try
        {
            using (var stream = new MemoryStream(fileBytes))
            {
                var uploadFileName = $"{secret.Folder}/{Path.GetFileName(filePath)}";
                await client.Files.UploadAsync(uploadFileName, WriteMode.Overwrite.Instance, body: stream);
                succeeded = true;
            }
        }
        catch
        {
            succeeded = false;
        }

        return succeeded;
    }
}