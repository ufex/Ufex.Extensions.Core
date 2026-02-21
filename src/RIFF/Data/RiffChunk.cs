using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data;

internal class RiffChunk : Chunk
{
	/// <summary>
	/// The FormatID is a 4-byte identifier that specifies the type of RIFF chunk. 
	/// For example, in a WAVE file, the FormatID would be "WAVE
	/// </summary>
	public Byte[] FormatID { get; init; }   // 4 bytes

	/// <summary>
	/// Chunks is a list of chunks that are contained within this RIFF chunk.
	/// </summary>
	public List<Chunk> Chunks { get; init; } = new List<Chunk>();

	/// <summary>
	/// A dictionary mapping chunk IDs to their corresponding chunk types. 
	/// This is used when reading sub-chunks to determine which class to 
	/// instantiate for each chunk based on its ID. For example, in a WAVE 
	/// RIFF chunk, the "fmt " sub-chunk would be mapped to the FmtChunk class.
	/// </summary>
	public virtual Dictionary<string, Type> ChunkTypes { get; } = new Dictionary<string, Type>();

	public RiffChunk(FileReader fr) : base(fr)
	{
		FormatID = fr.ReadBytes(4);
		Chunks = Chunk.ReadSubChunks(fr, Size - 4, ChunkTypes); // Subtract 4 bytes for the RIFF chunk ID that we just read
	}
}