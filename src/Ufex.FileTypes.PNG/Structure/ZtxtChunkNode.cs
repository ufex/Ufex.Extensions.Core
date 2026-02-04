using Ufex.API;
using Ufex.API.Tree;
using Ufex.FileTypes.PNG.Data;

namespace Ufex.FileTypes.PNG.Structure;

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
