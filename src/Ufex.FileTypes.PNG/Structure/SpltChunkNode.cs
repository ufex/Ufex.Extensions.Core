using System;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.FileTypes.PNG.Data;

namespace Ufex.FileTypes.PNG.Structure;

/// <summary>
/// sPLT - Suggested palette
/// </summary>
class SpltChunkNode : ChunkNode
{
	public SpltChunkNode(SpltChunk chunk) 
		: base(chunk, "sPLT", "Suggested palette", TreeViewIcon.Section)
	{
	}

	public override object[][] GetRows()
	{
		var d = (SpltChunk)Chunk;
		object[][] rows = [
			["Palette name", d.PaletteName],
			["Sample depth", d.SampleDepth],
		];
		return rows;
	}
}

