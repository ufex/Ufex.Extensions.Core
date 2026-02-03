using Ufex.API;

namespace Ufex.FileTypes.PNG.Data;

/// <summary>
/// IDAT - Image data
/// </summary>
internal class IdatChunk : Chunk
{
	public IdatChunk(FileReader fr) : base(fr)
	{
	}
}
