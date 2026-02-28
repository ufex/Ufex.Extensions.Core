using Ufex.API;
using Ufex.API.Tree;
using Ufex.Extensions.Core.PNG.Data;

namespace Ufex.Extensions.Core.PNG.Structure;

/// <summary>
/// sRGB - Standard RGB color space
/// </summary>
class SrgbChunkNode : ChunkNode
{
	public SrgbChunkNode(SrgbChunk chunk)
		: base(chunk, "sRGB", "Standard RGB color space", TreeViewIcon.Properties)
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
