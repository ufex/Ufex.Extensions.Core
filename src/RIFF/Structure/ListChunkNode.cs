using Ufex.API;
using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data;

namespace Ufex.Extensions.Core.RIFF.Structure;

/// <summary>
/// LIST - List chunk
/// </summary>
class ListChunkNode : ChunkNode
{
	public ListChunkNode(ListChunk chunk)
		: base(chunk, "LIST", "List chunk", TreeViewIcon.List)
	{
	}

	public override object[][] GetRows()
	{
		var d = (ListChunk)_chunk;
		object[][] rows = [
			["ListType", d.ListChunkID, d.ListChunkIDString]
		];
		return rows;
	}
}
