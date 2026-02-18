using Ufex.API;

namespace Ufex.Extensions.Core.PNG.Data;

/// <summary>
/// IDAT - Image data
/// </summary>
internal class IdatChunk : Chunk
{
	public IdatChunk(FileReader fr) : base(fr)
	{
	}
}
