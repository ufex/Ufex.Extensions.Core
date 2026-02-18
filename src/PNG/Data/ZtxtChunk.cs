using Ufex.API;

namespace Ufex.Extensions.Core.PNG.Data;

/// <summary>
/// zTXt - Compressed textual data
/// </summary>
internal class ZtxtChunk : Chunk
{
	public ZtxtChunk(FileReader fr) : base(fr)
	{
		// TODO: Implement zTXt parsing
		fr.BaseStream.Seek(Length, SeekOrigin.Current);
	}
}