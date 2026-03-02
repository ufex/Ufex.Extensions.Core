using Ufex.API.Format;
using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.WebP;

namespace Ufex.Extensions.Core.RIFF.Structure.WebP;

/// <summary>
/// Tree node for the XMP metadata chunk.
/// </summary>
class XmpChunkNode : ChunkNode
{
	public XmpChunkNode(XmpChunk chunk)
		: base(chunk, chunk.ChunkIDString, "XMP metadata", TreeViewIcon.Header)
	{
	}

	public override object[][] GetRows()
	{
		var d = (XmpChunk)_chunk;
		return [
			["DataOffset", (UInt32)d.DataOffset, "Byte offset of XMP data"],
			["DataSize", d.DataSize, ByteCountFormatter.Format(d.DataSize)]
		];
	}
}
