using Ufex.API.Format;
using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.WebP;

namespace Ufex.Extensions.Core.RIFF.Structure.WebP;

/// <summary>
/// Tree node for the VP8 lossy bitstream chunk.
/// </summary>
class Vp8ChunkNode : ChunkNode
{
	public Vp8ChunkNode(Vp8Chunk chunk)
		: base(chunk, chunk.ChunkIDString, "VP8 lossy bitstream", TreeViewIcon.Binary)
	{
	}

	public override object[][] GetRows()
	{
		var d = (Vp8Chunk)_chunk;
		return [
			["DataOffset", (UInt32)d.DataOffset, "Byte offset of VP8 data"],
		];
	}
}
