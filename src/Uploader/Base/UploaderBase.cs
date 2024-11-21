namespace Uploader.Base;

public abstract class UploaderBase
{
    public virtual Task<bool> UploadFile(string filePath)
    {
        throw new NotImplementedException();
    }
}