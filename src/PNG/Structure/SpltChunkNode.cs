using System;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.Extensions.Core.PNG.Data;

namespace Ufex.Extensions.Core.PNG.Structure;

/// <summary>
/// sPLT - Suggested palette
/// </summary>
class SpltChunkNode : ChunkNode
{
	public SpltChunkNode(SpltChunk chunk) 
		: base(chunk, "sPLT", "Suggested palette", TreeViewIcon.Palette)
	{
	}

	public override object[][] GetRows()
	{
		var d = (SpltChunk)Chunk;
		return [
			["Palette name", d.PaletteName],
			["Sample depth", d.SampleDepth],
		];
	}
}

