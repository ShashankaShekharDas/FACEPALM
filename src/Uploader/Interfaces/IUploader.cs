namespace Uploader.Interfaces;

public interface IUploader
{
    public Task<bool> UploadFile(string filePath);
}