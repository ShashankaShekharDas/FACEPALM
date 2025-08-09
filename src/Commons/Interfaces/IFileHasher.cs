namespace Commons.Interfaces;

public interface IFileHasher
{
    string ComputeHash(ReadOnlySpan<byte> data);
}