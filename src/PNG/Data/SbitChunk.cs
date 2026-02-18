using Ufex.API;

namespace Ufex.Extensions.Core.PNG.Data;

/// <summary>
/// sBIT - Significant bits
/// </summary>
internal class SbitChunk : Chunk
{
	public SbitChunk(FileReader fr) : base(fr)
	{
		// TODO: Implement sBIT parsing
		fr.BaseStream.Seek(Length, SeekOrigin.Current);
	}
}
