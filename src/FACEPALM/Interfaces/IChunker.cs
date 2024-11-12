namespace FACEPALM.Interfaces;

public interface IChunker
{
    public List<string> ChunkIncoming(string data);
}