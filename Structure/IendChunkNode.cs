using Ufex.API;
using Ufex.API.Tree;
using Ufex.FileTypes.PNG.Data;

namespace Ufex.FileTypes.PNG.Structure;

/// <summary>
/// IEND - Image trailer
/// </summary>
class IendChunkNode : ChunkNode
{
	public IendChunkNode(IendChunk chunk)
		: base(chunk, "IEND", "Image trailer", TreeViewIcon.Section)
	{
	}
}
