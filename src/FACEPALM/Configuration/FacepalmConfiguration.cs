using FACEPALM.Enums;

namespace FACEPALM.Configuration
{
    public class FacepalmConfiguration
    {
        public int DefaultChunkSize { get; set; } = 1_000_000;
        public long MinimumProviderSpace { get; set; } = 5_000_000;
        public string TempDirectory { get; set; } = Path.GetTempPath();
        public EncryptionSettings Encryption { get; set; } = new();
        public string? DefaultInputPath { get; set; }
        public FileType DefaultFileType { get; set; } = FileType.Folder;
        public EncryptionType DefaultEncryptionType { get; set; } = EncryptionType.Aes;
    }

    public class EncryptionSettings
    {
        public string KeyEnvironmentVariable { get; set; } = "FACEPALM_ENCRYPTION_KEY";
        public string IvEnvironmentVariable { get; set; } = "FACEPALM_ENCRYPTION_IV";
        public int DefaultKeySize { get; set; } = 256;
        public int DefaultIvSize { get; set; } = 128;
    }
}