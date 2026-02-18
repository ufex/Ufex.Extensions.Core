using Ufex.API;
using Ufex.API.Tree;
using Ufex.Extensions.Core.PNG.Data;

namespace Ufex.Extensions.Core.PNG.Structure;

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
