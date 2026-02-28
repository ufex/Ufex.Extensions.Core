using Ufex.API;
using Ufex.API.Tree;
using Ufex.Extensions.Core.PNG.Data;

namespace Ufex.Extensions.Core.PNG.Structure;

/// <summary>
/// cHRM - Primary chromaticities and white point
/// </summary>
class ChrmChunkNode : ChunkNode
{
	public ChrmChunkNode(ChrmChunk chunk)
		: base(chunk, "cHRM", "Primary chromaticities and white point", TreeViewIcon.Properties)
	{
	}

	public override object[][] GetRows()
	{
		var d = (ChrmChunk)Chunk;
		object[][] rows = [
			["White Point X", d.WhitePointX],
			["White Point Y", d.WhitePointY],
			["Red X", d.RedX],
			["Red Y", d.RedY],
			["Green X", d.GreenX],
			["Green Y", d.GreenY],
			["Blue X", d.BlueX],
			["Blue Y", d.BlueY],
		];
		return rows;
	}
}
