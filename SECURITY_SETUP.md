# FACEPALM Security Setup Guide

## Overview

FACEPALM now uses secure environment-based encryption key management instead of hardcoded keys. This guide will help you set up encryption keys securely.

## ⚠️ Important Security Notice

**NEVER** commit encryption keys to version control. Always use environment variables for sensitive data.

## Quick Setup

### 1. Generate Secure Keys

Run the following command to generate cryptographically secure keys:

```bash
dotnet run --generate-keys
```

This will output something like:
```
Generated secure encryption keys:
FACEPALM_ENCRYPTION_KEY=Base64EncodedKeyHere...
FACEPALM_ENCRYPTION_IV=Base64EncodedIVHere...
```

### 2. Set Environment Variables

#### On Linux/macOS:
```bash
export FACEPALM_ENCRYPTION_KEY="your-generated-key-here"
export FACEPALM_ENCRYPTION_IV="your-generated-iv-here"
```

To make these permanent, add them to your `~/.bashrc` or `~/.zshrc`:
```bash
echo 'export FACEPALM_ENCRYPTION_KEY="your-key"' >> ~/.bashrc
echo 'export FACEPALM_ENCRYPTION_IV="your-iv"' >> ~/.bashrc
source ~/.bashrc
```

#### On Windows:
```cmd
set FACEPALM_ENCRYPTION_KEY=your-generated-key-here
set FACEPALM_ENCRYPTION_IV=your-generated-iv-here
```

For permanent setup on Windows:
```cmd
setx FACEPALM_ENCRYPTION_KEY "your-generated-key-here"
setx FACEPALM_ENCRYPTION_IV "your-generated-iv-here"
```

### 3. Verify Setup

Run the application to verify your encryption keys are properly configured:

```bash
dotnet run --path "/path/to/your/files"
```

## Usage Examples

### Basic Usage
```bash
dotnet run --path "/home/user/documents"
```

### Advanced Usage
```bash
dotnet run --path "/home/user/documents" --file-type Folder --chunk-size 2000000 --encryption Aes
```

### Available Command Line Options

- `--path`: Path to file or folder to process (required)
- `--file-type`: File or Folder (default: Folder)
- `--chunk-size`: Chunk size in bytes (default: 1000000)
- `--encryption`: Encryption type (default: Aes)
- `--generate-keys`: Generate new encryption keys

## Security Best Practices

1. **Key Rotation**: Regularly generate new encryption keys
2. **Backup Keys Securely**: Store keys in a secure password manager
3. **Environment Isolation**: Use different keys for development and production
4. **Access Control**: Limit who can access the environment variables
5. **Audit Trail**: Monitor access to encrypted files

## Troubleshooting

### "Security Error: Environment variable not set"
- Ensure you've set both `FACEPALM_ENCRYPTION_KEY` and `FACEPALM_ENCRYPTION_IV`
- Verify the environment variables are available in your current shell session

### "Encryption key is too short"
- Use the `--generate-keys` command to create properly sized keys
- Ensure you're using the full base64-encoded key

### "Initialization vector must be exactly 128 bits"
- The IV must be exactly 16 bytes when base64 decoded
- Use the generated IV from `--generate-keys` command

## Migration from Hardcoded Keys

If you're upgrading from a version with hardcoded keys:

1. Generate new secure keys using `--generate-keys`
2. Set the environment variables as described above
3. Re-encrypt any existing data with the new keys
4. Remove any old hardcoded keys from your configuration

## Support

For security-related issues, please review this guide first. If you still need help, create an issue with the "security" label (but never include actual keys in the issue).