using Ufex.API;
using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data;

namespace Ufex.Extensions.Core.RIFF.Structure;

/// <summary>
/// ZStr - Null terminated string
/// </summary>
class ZStrChunkNode : ChunkNode
{
	public ZStrChunkNode(ZStrChunk chunk)
		: base(chunk, chunk.ChunkIDString, "Null terminated string", TreeViewIcon.Text)
	{
	}

	public override object[][] GetRows()
	{
		var d = (ZStrChunk)_chunk;
		object[][] rows = [
			["Text", d.Text, d.TextString]
		];
		return rows;
	}
}
