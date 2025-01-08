# Google Drive Uploader

Must have a google account before proceeding further

## Configuring service account to upload

- Go to Google cloud console
- Create a new Service Account in the Google Cloud Console. It will be in this format <>
  @homelab-facepalm.iam.gserviceaccount.com
- Go inside the Service Account
- Top toolbar, click on Keys.
- Then Add Key -> Create new Key -> JSON format
- Create a folder in Google Drive, the ID at the top after folders is the folder id.
- Add your service account to have read/write access to the folder (Just read if you just want the svc account to read)
- To Add the service account, click on the folder name -> Share -> Share -> Add your svc account email address as
  Editor.
- The following code snippet to upload file to GoogleDrive

 ```
 
        // To store the secret in database
        var googleDriveSecret = new GoogleDriveSecret("secret.json", File.ReadAllText("secret.json"), "folder-id");
        
        var credentialStoreObject = new CredentialStore(Guid.NewGuid().ToString(), StorageProviderTypes.GoogleDrive, GoogleDriveSecret.GetSerializedContent(googleDriveSecret), 10000000000, 0);
        
        await CredentialStore.UploadSecretToDatabaseAsync(credentialStoreObject);
        
        // To Upload a file
        var googleDriveUploader = new GoogleDriveUploader(
            GoogleDriveUploader.GenerateStreamFromString(File.ReadAllText("secret.json")), // Or read from database
            "folder-id" 
        );

        await googleDriveUploader.UploadFile("file-to-upload");
 ```