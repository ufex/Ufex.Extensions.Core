using Ufex.API.Format;
using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.WebP;

namespace Ufex.Extensions.Core.RIFF.Structure.WebP;

/// <summary>
/// Tree node for the VP8L lossless bitstream chunk.
/// </summary>
class Vp8lChunkNode : ChunkNode
{
	public Vp8lChunkNode(Vp8lChunk chunk)
		: base(chunk, chunk.ChunkIDString, "VP8L lossless bitstream", TreeViewIcon.Binary)
	{
	}

	public override object[][] GetRows()
	{
		var d = (Vp8lChunk)_chunk;
		return [
			["DataOffset", (UInt32)d.DataOffset, "Byte offset of VP8L data"],
		];
	}
}
