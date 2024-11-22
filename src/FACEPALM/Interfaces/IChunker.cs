namespace FACEPALM.Interfaces;

public interface IChunker
{
    public IEnumerable<string> ChunkIncoming(string data, int chunkSize = 1000000);
}