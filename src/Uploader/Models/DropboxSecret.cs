using System.Text.Json;
using Uploader.Base;

namespace Uploader.Models;

public class DropboxSecret(string refreshToken, string appKey, string appSecret, string folderName) : IPortalSecret
{
    public string RefreshToken { get; } = refreshToken;
    public string AppKey { get; } = appKey;
    public string AppSecret { get; } = appSecret;
    public string Folder { get; } = folderName;

    //ISSUE: Cannot use JsonSerializer
    public static string GetSerializedContent(DropboxSecret dropboxSecretObject)
    {
        return JsonSerializer.Serialize(dropboxSecretObject);
    }

    public static DropboxSecret? GetDeserializedContent(string serializedSecretObject)
    {
        return JsonSerializer.Deserialize<DropboxSecret>(serializedSecretObject);
    }
}