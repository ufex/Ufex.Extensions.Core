using Ufex.API;
using Ufex.API.Tree;
using Ufex.Extensions.Core.PNG.Data;

namespace Ufex.Extensions.Core.PNG.Structure;

/// <summary>
/// gAMA - Image gamma
/// </summary>
class GamaChunkNode : ChunkNode
{
	public GamaChunkNode(GamaChunk chunk)
		: base(chunk, "gAMA", "Image gamma", TreeViewIcon.Properties)
	{
	}

	public override object[][] GetRows()
	{
		var d = (GamaChunk)Chunk;
		return [
			["Gamma", d.Gamma],
		];
	}
}
