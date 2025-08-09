using Commons.Interfaces;
using System.Security.Cryptography;

namespace Commons.Hashers;

public class Sha256Base64Hasher : IFileHasher
{
    public string ComputeHash(ReadOnlySpan<byte> data)
    {
        var hash = SHA256.HashData(data);
        return Convert.ToBase64String(hash);

    }
}