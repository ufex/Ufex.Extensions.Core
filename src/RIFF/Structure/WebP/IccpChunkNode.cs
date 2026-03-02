using Ufex.API.Format;
using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.WebP;

namespace Ufex.Extensions.Core.RIFF.Structure.WebP;

/// <summary>
/// Tree node for the ICCP color profile chunk.
/// </summary>
class IccpChunkNode : ChunkNode
{
	public IccpChunkNode(IccpChunk chunk)
		: base(chunk, chunk.ChunkIDString, "ICC color profile", TreeViewIcon.Header)
	{
	}

	public override object[][] GetRows()
	{
		var d = (IccpChunk)_chunk;
		return [
			["DataOffset", (UInt32)d.DataOffset, "Byte offset of ICC profile data"],
			["DataSize", d.DataSize, ByteCountFormatter.Format(d.DataSize)]
		];
	}
}
