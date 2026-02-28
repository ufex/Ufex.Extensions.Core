using Ufex.API;
using Ufex.API.Tree;
using Ufex.Extensions.Core.PNG.Data;

namespace Ufex.Extensions.Core.PNG.Structure;

/// <summary>
/// IDAT - Image data
/// </summary>
class IdatChunkNode : ChunkNode
{
	public IdatChunkNode(IdatChunk chunk)
		: base(chunk, "IDAT", "Image data", TreeViewIcon.Binary)
	{
	}
}
