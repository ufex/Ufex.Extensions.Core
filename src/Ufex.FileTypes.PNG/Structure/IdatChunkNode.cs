using Ufex.API;
using Ufex.API.Tree;
using Ufex.FileTypes.PNG.Data;

namespace Ufex.FileTypes.PNG.Structure;

/// <summary>
/// IDAT - Image data
/// </summary>
class IdatChunkNode : ChunkNode
{
	public IdatChunkNode(IdatChunk chunk)
		: base(chunk, "IDAT", "Image data", TreeViewIcon.Section)
	{
	}
}
