using System.Text.Json;

namespace Uploader.Models;

//NOT DATABASE CLASS
public class Secret (string fileName, string content)
{
    public string FileName { get;} = fileName;
    public string FileContent { get;} = content;
    public string? FolderId { get; set; } // specifically for google drive

    public static string GetSerializedContent(Secret secretObject) => JsonSerializer.Serialize(secretObject);
    public static Secret? GetDeserializedContent(string serializedSecretObject) => JsonSerializer.Deserialize<Secret>(serializedSecretObject);
}