namespace FileHandler.Base
{
    public abstract class FileHandlerBase
    {
        public virtual Task<bool> UploadFile(string filePath)
        {
            throw new NotImplementedException();
        }

        public abstract Task<string> DownloadFile(string fileId, string downloadedFileName);
    }
}