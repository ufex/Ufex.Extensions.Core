using Ufex.API;
using Ufex.API.Tree;
using Ufex.Extensions.Core.PNG.Data;

namespace Ufex.Extensions.Core.PNG.Structure;

/// <summary>
/// sBIT - Significant bits
/// </summary>
class SbitChunkNode : ChunkNode
{
	public SbitChunkNode(SbitChunk chunk)
		: base(chunk, "sBIT", "Significant bits", TreeViewIcon.Properties)
	{
	}
}
