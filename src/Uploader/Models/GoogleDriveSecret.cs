using System.Text.Json;
using Uploader.Interfaces;

namespace Uploader.Models;

//NOT DATABASE CLASS
public class GoogleDriveSecret (string fileName, string content, string folderId) : IPortalSecret
{
    public string FileName { get;} = fileName;
    public string FileContent { get;} = content;
    public string FolderId { get; } = folderId;

    //ISSUE: Cannot use JsonSerializer
    public static string GetSerializedContent(GoogleDriveSecret googleDriveSecretObject) => JsonSerializer.Serialize(googleDriveSecretObject);
    public static GoogleDriveSecret? GetDeserializedContent(string serializedSecretObject) => JsonSerializer.Deserialize<GoogleDriveSecret>(serializedSecretObject);
}