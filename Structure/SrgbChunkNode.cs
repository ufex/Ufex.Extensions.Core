using Ufex.API;
using Ufex.API.Tree;
using Ufex.FileTypes.PNG.Data;

namespace Ufex.FileTypes.PNG.Structure;

/// <summary>
/// sRGB - Standard RGB color space
/// </summary>
class SrgbChunkNode : ChunkNode
{
	public SrgbChunkNode(SrgbChunk chunk)
		: base(chunk, "sRGB", "Standard RGB color space", TreeViewIcon.Section)
	{
	}

	public override object[][] GetRows()
	{
		var d = (SrgbChunk)Chunk;
		return [
			["Rendering Intent", d.RenderingIntent],
		];
	}
}
