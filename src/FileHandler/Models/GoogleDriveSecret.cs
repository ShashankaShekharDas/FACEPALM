using System.Text.Json;
using FileHandler.Base;

namespace FileHandler.Models
{
    //NOT DATABASE CLASS
    public class GoogleDriveSecret(string fileName, string content, string folderId) : IPortalSecret
    {
        public string FileName { get; } = fileName;
        public string FileContent { get; } = content;
        public string FolderId { get; } = folderId;

        //ISSUE: Cannot use JsonSerializer
        public static string GetSerializedContent(GoogleDriveSecret googleDriveSecretObject)
        {
            return JsonSerializer.Serialize(googleDriveSecretObject);
        }

        public static GoogleDriveSecret GetDeserializedContent(string serializedSecretObject)
        {
            var jsonDeserializedObject = JsonSerializer.Deserialize<Dictionary<string, string>>(serializedSecretObject);

            if (jsonDeserializedObject == null)
            {
                throw new ArgumentException("serializedSecretObject is empty", nameof(serializedSecretObject));
            }

            jsonDeserializedObject.TryGetValue("FileName", out var fileName);
            jsonDeserializedObject.TryGetValue("FileContent", out var fileContent);
            jsonDeserializedObject.TryGetValue("FolderId", out var folderId);

            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(fileContent) || string.IsNullOrEmpty(folderId))
            {
                throw new ArgumentException("serializedSecretObject is invalid", nameof(serializedSecretObject));
            }

            return new GoogleDriveSecret(fileName, fileContent, folderId);
        }
    }
}