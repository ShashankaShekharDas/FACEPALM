using FACEPALM.Interfaces;

namespace FACEPALM.Chunker;

public sealed class Chunker : IChunker
{
    private readonly int _chunkSize = 1024 * 1024;

    #region Constructors

    public Chunker()
    {
    }

    public Chunker(int chunkSize)
    {
        _chunkSize = chunkSize;
    }

    #endregion

    public List<string> ChunkIncoming(string data)
    {
        return data.Split("", _chunkSize).ToList();
    }
}