using Ufex.API;
using Ufex.API.Tree;
using Ufex.Extensions.Core.PNG.Data;

namespace Ufex.Extensions.Core.PNG.Structure;

/// <summary>
/// zTXt - Compressed textual data
/// </summary>
class ZtxtChunkNode : ChunkNode
{
	public ZtxtChunkNode(ZtxtChunk chunk)
		: base(chunk, "zTXt", "Compressed textual data", TreeViewIcon.Section)
	{
	}
}
