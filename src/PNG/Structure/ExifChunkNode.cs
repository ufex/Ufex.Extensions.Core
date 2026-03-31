using Ufex.API.Tree;
using Ufex.Extensions.Core.EXIF.Data;
using Ufex.Extensions.Core.EXIF.Structure;
using Ufex.Extensions.Core.PNG.Data;

namespace Ufex.Extensions.Core.PNG.Structure;

/// <summary>
/// eXIf - Exchangeable image file format metadata.
/// </summary>
internal class ExifChunkNode : ChunkNode
{
	public ExifChunkNode(ExifChunk chunk, ExifData? exifData)
		: base(chunk, "eXIf", "Exif metadata", TreeViewIcon.Properties)
	{
		if (exifData != null)
			Nodes.Add(new ExifNode(exifData));
	}

	public override object[][] GetRows()
	{
		var d = (ExifChunk)Chunk;
		return [
			["Data", d.ExifData, $"{d.ExifData.Length} bytes"],
		];
	}
}
