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

    public static DropboxSecret GetDeserializedContent(string serializedSecretObject)
    {
        var jsonDeserializedObject = JsonSerializer.Deserialize<Dictionary<string, string>>(serializedSecretObject);

        if (jsonDeserializedObject == null)
        {
            throw new ArgumentException("serializedSecretObject is empty", nameof(serializedSecretObject));
        }
        
        jsonDeserializedObject.TryGetValue("RefreshToken", out var refreshToken);
        jsonDeserializedObject.TryGetValue("AppKey", out var appKey);
        jsonDeserializedObject.TryGetValue("AppSecret", out var appSecret);
        jsonDeserializedObject.TryGetValue("Folder", out var folder);

        if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(appKey) || string.IsNullOrEmpty(appSecret) || string.IsNullOrEmpty(folder))
        {
            throw new ArgumentException("serializedSecretObject is invalid", nameof(serializedSecretObject));    
        }
        
        return new DropboxSecret(refreshToken, appKey, appSecret, folder);
    }
}