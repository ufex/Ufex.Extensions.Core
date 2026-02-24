using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data;

/// <summary>
/// Base class for RIFF chunks. Typically a subclass of RiffChunk 
/// will represent a specific RIFF format (e.g. WAVE, AVI, etc.) and will 
/// define the ChunkTypes dictionary to specify which chunk types are valid 
/// for that format. The RiffChunk class itself can be used for generic RIFF 
/// chunks where the specific format has not been implemented.
/// 
/// A RIFF chunk contains a FormatID that specifies the type of chunk, 
/// and a list of sub-chunks that are contained within it. The ChunkTypes 
/// dictionary is used to determine which class to instantiate for each 
/// sub-chunk based on its ID when reading the chunk data. For example, in 
/// a WAVE RIFF chunk, the "fmt " sub-chunk would be mapped to the FmtChunk class.
/// </summary>
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