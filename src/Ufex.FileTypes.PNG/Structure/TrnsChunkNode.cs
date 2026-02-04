using System.Collections.Generic;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.FileTypes.PNG.Data;

namespace Ufex.FileTypes.PNG.Structure;

/// <summary>
/// tRNS - Transparency
/// </summary>
class TrnsChunkNode : ChunkNode
{
	public TrnsChunkNode(TrnsChunk chunk)
		: base(chunk, "tRNS", "Transparency", TreeViewIcon.Section)
	{
	}

	public override object[][] GetRows()
	{
		var d = (TrnsChunk)Chunk;
		var rowList = new List<object[]>();

		if (d.ColorType == 0)
		{
			rowList.Add(["Grey sample value", d.GreySampleValue]);
		}
		else if (d.ColorType == 2)
		{
			rowList.Add(["Red sample value", d.RedSampleValue]);
			rowList.Add(["Green sample value", d.GreenSampleValue]);
			rowList.Add(["Blue sample value", d.BlueSampleValue]);
		}
		else if (d.ColorType == 3)
		{
			for (int i = 0; i < d.PaletteAlpha.Length; i++)
			{
				rowList.Add(["Alpha for palette index " + i.ToString(), d.PaletteAlpha[i]]);
			}
		}

		return rowList.ToArray();
	}
}
