using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.Wave;

namespace Ufex.Extensions.Core.RIFF.Structure.Wave;

/// <summary>
/// Tree node for the fact chunk.
/// </summary>
class FactChunkNode : ChunkNode
{
	public FactChunkNode(FactChunk chunk)
		: base(chunk, chunk.ChunkIDString, "Fact chunk", TreeViewIcon.Header)
	{
	}

	public override object[][] GetRows()
	{
		var d = (FactChunk)_chunk;
		return [
			["SampleLength", d.SampleLength, $"{d.SampleLength} samples"]
		];
	}
}
