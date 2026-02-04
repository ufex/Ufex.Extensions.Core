using Ufex.API;

namespace Ufex.FileTypes.PNG.Data;

/// <summary>
/// IEND - Image trailer
/// </summary>
internal class IendChunk : Chunk
{
	public IendChunk(FileReader fr) : base(fr)
	{
	}
}
