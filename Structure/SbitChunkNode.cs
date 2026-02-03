using Ufex.API;
using Ufex.API.Tree;
using Ufex.FileTypes.PNG.Data;

namespace Ufex.FileTypes.PNG.Structure;

/// <summary>
/// sBIT - Significant bits
/// </summary>
class SbitChunkNode : ChunkNode
{
	public SbitChunkNode(SbitChunk chunk)
		: base(chunk, "sBIT", "Significant bits", TreeViewIcon.Section)
	{
	}
}
