using FACEPALM.Interfaces;

namespace FACEPALM.Chunker;

public sealed class Chunker : IChunker
{
    public IEnumerable<string> ChunkIncoming(string data, int chunkSize = 1000000)
    {
        for (var i = 0; i < data.Length; i += chunkSize)
        {
            if (i + chunkSize <= data.Length)
            {
                yield return data.Substring(i, chunkSize);
            }
            else
            {
                yield return data[i..];
            }
        }
    }
}