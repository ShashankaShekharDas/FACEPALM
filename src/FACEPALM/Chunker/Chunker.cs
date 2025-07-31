using System.Buffers;
using FACEPALM.Interfaces;

namespace FACEPALM.Chunker
{
    public class Chunker : IChunker
    {
        public IEnumerable<string> ChunkIncoming(string data, int chunkSize = 1000000)
        {
            ArgumentNullException.ThrowIfNull(data, nameof(data));
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize, nameof(chunkSize));

            if (chunkSize == 0 || data.Length <= chunkSize)
            {
                yield return data;
                yield break;
            }

            // Use ReadOnlySpan for memory-efficient chunking
            var span = data.AsSpan();
            for (var i = 0; i < span.Length; i += chunkSize)
            {
                var length = Math.Min(chunkSize, span.Length - i);
                yield return span.Slice(i, length).ToString();
            }
        }

        /// <summary>
        /// Memory-efficient chunking using ReadOnlyMemory for large data sets
        /// Reduces string allocations by working with memory spans
        /// </summary>
        public IEnumerable<ReadOnlyMemory<char>> ChunkIncomingMemory(ReadOnlyMemory<char> data, int chunkSize = 1000000)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize, nameof(chunkSize));

            if (chunkSize == 0 || data.Length <= chunkSize)
            {
                yield return data;
                yield break;
            }

            for (var i = 0; i < data.Length; i += chunkSize)
            {
                var length = Math.Min(chunkSize, data.Length - i);
                yield return data.Slice(i, length);
            }
        }

        /// <summary>
        /// Stream-based chunking for very large data that doesn't fit in memory
        /// Processes data in chunks without loading everything into memory
        /// </summary>
        public async IAsyncEnumerable<string> ChunkStreamAsync(Stream inputStream, int chunkSize = 1000000)
        {
            ArgumentNullException.ThrowIfNull(inputStream, nameof(inputStream));
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize, nameof(chunkSize));

            if (!inputStream.CanRead)
            {
                throw new ArgumentException("Stream must be readable", nameof(inputStream));
            }

            var buffer = ArrayPool<byte>.Shared.Rent(chunkSize);
            try
            {
                int bytesRead;
                while ((bytesRead = await inputStream.ReadAsync(buffer, 0, chunkSize)) > 0)
                {
                    // Convert bytes to base64 string for consistency with existing code
                    var chunk = Convert.ToBase64String(buffer, 0, bytesRead);
                    yield return chunk;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}