using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.Wave;

namespace Ufex.Extensions.Core.RIFF.Structure.Wave;

/// <summary>
/// Tree node for the label chunk.
/// </summary>
class LablChunkNode : ChunkNode
{
	public LablChunkNode(LablChunk chunk)
		: base(chunk, chunk.ChunkIDString, "Label", TreeViewIcon.Text)
	{
	}

	public override object[][] GetRows()
	{
		var d = (LablChunk)_chunk;
		return [
			["CuePointID", d.CuePointID],
			["Text", d.Text, d.TextString]
		];
	}
}
