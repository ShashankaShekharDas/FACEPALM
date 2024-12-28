using Uploader.Models;

namespace FACEPALM.Interfaces
{
    internal interface IFacepalm
    {
        public Task<Dictionary<string, bool>> UploadFolder(string path);
        public Task<bool> UploadFile(string path);
        public Task<bool> UploadFile(string path, CredentialStore credentialStore);
    }
}