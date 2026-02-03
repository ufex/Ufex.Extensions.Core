using Ufex.API;
using Ufex.API.Tree;
using Ufex.FileTypes.PNG.Data;

namespace Ufex.FileTypes.PNG.Structure;

/// <summary>
/// gAMA - Image gamma
/// </summary>
class GamaChunkNode : ChunkNode
{
	public GamaChunkNode(GamaChunk chunk)
		: base(chunk, "gAMA", "Image gamma", TreeViewIcon.Section)
	{
	}

	public override object[][] GetRows()
	{
		var d = (GamaChunk)Chunk;
		object[][] rows = [
			["Gamma", d.Gamma],
		];
		return rows;
	}
}
