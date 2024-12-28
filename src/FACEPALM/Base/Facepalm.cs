using System.ComponentModel;
using Commons.Constants;
using Commons.Database;
using Commons.Database.Handlers;
using FACEPALM.Exceptions;
using FACEPALM.Interfaces;
using Uploader.Factory;
using Uploader.Models;

namespace FACEPALM.Base
{
    [Description("Facepalm class ONLY HANDLES CHUNKED AND ENCRYPTED FILES. That is why internal")]
    internal sealed class Facepalm : IFacepalm
    {
        private readonly GenericPostgresDatabaseHelper<CredentialStore> _credentialStoreProvider = new();

        private static List<string> GetAllFilesInDirectory(string directory)
        {
            return Directory.GetFiles(directory).ToList();
        }

        // ISSUE : FOR NOW CHUNK SIZE IS HARDCODED TO 1,000,000 BITS OR 1 MB
        public async Task<Dictionary<string, bool>> UploadFolder(string path)
        {
            var validProviders = await _credentialStoreProvider.SearchRows(
                [new WhereClause("maxsizeinbytes - usedsizeinbytes", "5000000", DatabaseOperator.GreaterThanOrEqual)],
                CredentialStore.Deserialize);

            var allFilesInDirectory = GetAllFilesInDirectory(path);
            var uploadResult = new Dictionary<string, bool>();
            var providerIndex = 0;

            foreach (var file in allFilesInDirectory)
            {
                var provider = validProviders[providerIndex % validProviders.Count];

                while (provider.UsedSizeInBytes + 1000000 > provider.MaxSizeInBytes)
                {
                    providerIndex++;
                    provider = validProviders[providerIndex % validProviders.Count];
                }

                uploadResult[file] = await Task.Run(async () => await UploadFile(file, provider));

                if (uploadResult[file])
                {
                    provider.UsedSizeInBytes += 1000000;
                }

                providerIndex++;
            }

            return uploadResult;
        }

        // Try each file till failure
        public async Task<bool> UploadFile(string path)
        {
            var validProviders = await _credentialStoreProvider.SearchRows(
                [new WhereClause("maxsizeinbytes - usedsizeinbytes", "5000000", DatabaseOperator.GreaterThanOrEqual)],
                CredentialStore.Deserialize);

            if (validProviders.Count == 0)
            {
                throw new NoValidProviderException("No valid providers found");
            }

            return await UploadFile(path, validProviders.First());
        }

        public async Task<bool> UploadFile(string path, CredentialStore credentialStore)
        {
            return await UploaderFactory.GetUploader(credentialStore).UploadFile(path);
        }
    }
}