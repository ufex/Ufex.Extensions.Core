using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data;

/// <summary>
/// Represents a LIST chunk in a RIFF file. A LIST chunk contains a list of sub-chunks, 
/// each of which can be of a different type. This class is typically used as a base
/// class for specific types of LIST chunks, such as the INFO chunk in WAVE files, 
/// which contains metadata about the audio file. 
/// 
/// The ListChunk class reads the ListChunkID to determine the type of LIST chunk, and 
/// then reads the sub-chunks contained within it based on the specified size. The 
/// SubChunkTypes dictionary can be overridden in subclasses to specify which types 
/// of sub-chunks are valid for that specific type of LIST chunk.
/// </summary>
internal class ListChunk : Chunk
{
	/// <summary>
	/// The LIST chunk ID is a 4-byte identifier that specifies the type of LIST chunk. 
	/// For example, in a WAVE file, the LIST chunk ID could be "INFO" for metadata, 
	/// or "adtl" for associated data list. The specific meaning of the LIST chunk 
	/// depends on the context in which it is used.
	/// </summary>
	public Byte[] ListChunkID { get; init; }   // 4 bytes
	public List<Chunk> SubChunks { get; init; } = [];

	public string ListChunkIDString => System.Text.Encoding.ASCII.GetString(ListChunkID);

	/// <summary>
	/// A dictionary mapping sub-chunk IDs to their corresponding chunk types. 
	/// This is used when reading sub-chunks to determine which class to instantiate 
	/// for each sub-chunk based on its ID.
	/// </summary>
	public virtual Dictionary<string, Type> SubChunkTypes { get; } = new Dictionary<string, Type>();

	public ListChunk(FileReader fr) : base(fr)
	{
		ListChunkID = fr.ReadBytes(4);
		SubChunks = Chunk.ReadSubChunks(fr, Size - 4, SubChunkTypes); // Subtract 4 bytes for the List chunk ID that we just read
	}
}