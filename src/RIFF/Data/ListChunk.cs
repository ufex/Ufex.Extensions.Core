using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data;

internal class ListChunk : Chunk
{
	public Byte[] ListChunkID { get; init; }   // 4 bytes
	public List<Chunk> SubChunks { get; init; } = [];

	public string ListChunkIDString => System.Text.Encoding.ASCII.GetString(ListChunkID);
	public virtual Dictionary<string, Type> SubChunkTypes { get; } = new Dictionary<string, Type>();

	public ListChunk(FileReader fr) : base(fr)
	{
		ListChunkID = fr.ReadBytes(4);
		SubChunks = Chunk.ReadSubChunks(fr, Size - 4, SubChunkTypes); // Subtract 4 bytes for the List chunk ID that we just read
	}
}