using Ufex.API.Format;
using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.WebP;

namespace Ufex.Extensions.Core.RIFF.Structure.WebP;

/// <summary>
/// Tree node for the EXIF metadata chunk.
/// </summary>
class ExifChunkNode : ChunkNode
{
	public ExifChunkNode(ExifChunk chunk)
		: base(chunk, chunk.ChunkIDString, "Exif metadata", TreeViewIcon.Header)
	{
	}

	public override object[][] GetRows()
	{
		var d = (ExifChunk)_chunk;
		return [
			["DataOffset", (UInt32)d.DataOffset, "Byte offset of Exif data"],
			["DataSize", d.DataSize, ByteCountFormatter.Format(d.DataSize)]
		];
	}
}
