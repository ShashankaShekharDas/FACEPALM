using Commons.Constants;
using FACEPALM.Base;
using FACEPALM.Configuration;
using FACEPALM.Enums;
using FACEPALM.Services;

namespace FACEPALM
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            /*
             * Workflow
             * Get File
             * Encrypt
             * Chunk
             * Filename : <filename> - <chunk number> - <total chunks> . shas
             * Store
             * Metadata of where each file is stored
             */

            try
            {
                // Load configuration
                var configuration = LoadConfiguration(args);
                
                // Initialize encryption key manager
                var encryptionKeyManager = new EncryptionKeyManager(configuration.Encryption);
                encryptionKeyManager.ValidateEncryptionEnvironment();

                // Get secure encryption parameters from environment variables
                var encryptionParameters = encryptionKeyManager.GetEncryptionParameters();

                var coldStoragePreparator = new ColdStoragePreparator();
                var facepalmObject = new Facepalm();

                // Use configuration instead of hardcoded values
                var inputPath = configuration.DefaultInputPath ?? GetInputPathFromArgs(args);
                if (string.IsNullOrEmpty(inputPath))
                {
                    Console.WriteLine("Error: No input path specified. Use --path argument or set DefaultInputPath in configuration.");
                    Environment.Exit(1);
                }

                var temporaryDirectory = await coldStoragePreparator.PrepareFileForStorage(
                    inputPath,
                    configuration.DefaultFileType,
                    configuration.DefaultEncryptionType,
                    configuration.DefaultChunkSize,
                    encryptionParameters);

                await facepalmObject.UploadFolder(temporaryDirectory);

                Console.WriteLine("Upload completed successfully.");
            }
            catch (SecurityException ex)
            {
                Console.WriteLine($"Security Error: {ex.Message}");
                Console.WriteLine("\nTo set up encryption keys:");
                Console.WriteLine("1. Generate secure keys using: dotnet run --generate-keys");
                Console.WriteLine("2. Set environment variables:");
                Console.WriteLine("   export FACEPALM_ENCRYPTION_KEY='<your-base64-key>'");
                Console.WriteLine("   export FACEPALM_ENCRYPTION_IV='<your-base64-iv>'");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private static FacepalmConfiguration LoadConfiguration(string[] args)
        {
            var configuration = new FacepalmConfiguration();

            // Handle special commands
            if (args.Contains("--generate-keys"))
            {
                var (key, iv) = EncryptionKeyManager.GenerateSecureKeyPair();
                Console.WriteLine("Generated secure encryption keys:");
                Console.WriteLine($"FACEPALM_ENCRYPTION_KEY={key}");
                Console.WriteLine($"FACEPALM_ENCRYPTION_IV={iv}");
                Console.WriteLine("\nSet these as environment variables before running the application.");
                Environment.Exit(0);
            }

            // Parse command line arguments for configuration overrides
            for (int i = 0; i < args.Length - 1; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--chunk-size":
                        if (int.TryParse(args[i + 1], out var chunkSize))
                            configuration.DefaultChunkSize = chunkSize;
                        break;
                    case "--file-type":
                        if (Enum.TryParse<FileType>(args[i + 1], true, out var fileType))
                            configuration.DefaultFileType = fileType;
                        break;
                    case "--encryption":
                        if (Enum.TryParse<EncryptionType>(args[i + 1], true, out var encryptionType))
                            configuration.DefaultEncryptionType = encryptionType;
                        break;
                }
            }

            return configuration;
        }

        private static string? GetInputPathFromArgs(string[] args)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].ToLower() == "--path")
                {
                    return args[i + 1];
                }
            }
            return null;
        }
    }
}