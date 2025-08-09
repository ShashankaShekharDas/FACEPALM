using System.Security.Cryptography;
using Commons.Interfaces;

namespace Commons.Hashers
{
    public sealed class Sha512Base64Hasher : IFileHasher
    {
        public string ComputeHash(ReadOnlySpan<byte> data)
        {
            var hash = SHA512.HashData(data);
            return Convert.ToBase64String(hash);
        }
    }
}