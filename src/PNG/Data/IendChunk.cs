using Ufex.API;

namespace Ufex.Extensions.Core.PNG.Data;

/// <summary>
/// IEND - Image trailer
/// </summary>
internal class IendChunk : Chunk
{
	public IendChunk(FileReader fr) : base(fr)
	{
	}
}
