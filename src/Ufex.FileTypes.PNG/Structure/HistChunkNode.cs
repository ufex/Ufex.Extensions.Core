using System;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.FileTypes.PNG.Data;

namespace Ufex.FileTypes.PNG.Structure;

/// <summary>
/// hIST - Image histogram
/// </summary>
class HistChunkNode : ChunkNode
{
	public HistChunkNode(HistChunk chunk) 
		: base(chunk, "hIST", "Image histogram", TreeViewIcon.Section)
	{
	}

	public override object[][] GetRows()
	{
		var d = (HistChunk)Chunk;
		var rows = new List<object[]>();

		for (int i = 0; i < d.Frequencies.Length; i++)
		{
			rows.Add([$"Frequency [{i}]", d.Frequencies[i]]);
		}

		return rows.ToArray();
	}
}