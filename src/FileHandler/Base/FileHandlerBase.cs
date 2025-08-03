namespace FileHandler.Base
{
    public interface IFileHandlerBase
    {
        public abstract Task<bool> UploadFile(string filePath);

        public abstract Task<string> DownloadFile(string fileId, string downloadedFileName);
    }
}