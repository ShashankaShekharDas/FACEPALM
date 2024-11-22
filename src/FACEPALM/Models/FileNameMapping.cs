using Commons.Interfaces;

namespace FACEPALM.Models;

public class FileNameMapping(string fileName, string serializedName, long totalChunks) : IDatabaseModels
{
    public string FileName { get; set; } = fileName;
    public string SerializedName { get; set; } = serializedName;
    public long TotalChunks { get; set; } = totalChunks;
}