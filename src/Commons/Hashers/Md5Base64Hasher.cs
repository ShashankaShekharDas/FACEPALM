using System.Security.Cryptography;
using Commons.Interfaces;

namespace Commons.Hashers
{
    public sealed class Md5Base64Hasher : IFileHasher
    {
        public string ComputeHash(ReadOnlySpan<byte> data)
        {
            var hash = MD5.HashData(data);
            return Convert.ToBase64String(hash);
        }
    }
}