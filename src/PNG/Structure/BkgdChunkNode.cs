using System.Collections.Generic;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.Extensions.Core.PNG.Data;

namespace Ufex.Extensions.Core.PNG.Structure;

/// <summary>
/// bKGD - Background colour
/// </summary>
class BkgdChunkNode : ChunkNode
{
	public BkgdChunkNode(BkgdChunk chunk)
		: base(chunk, "bKGD", "Background color", TreeViewIcon.Section)
	{
	}

	public override object[][] GetRows()
	{
		var d = (BkgdChunk)Chunk;
		var rowList = new List<object[]>();

		if (d.ColorType == 0 || d.ColorType == 4)
		{
			rowList.Add(["Greyscale", d.Greyscale]);
		}
		else if (d.ColorType == 2 || d.ColorType == 6)
		{
			rowList.Add(["Red", d.Red]);
			rowList.Add(["Green", d.Green]);
			rowList.Add(["Blue", d.Blue]);
		}
		else if (d.ColorType == 3)
		{
			rowList.Add(["Palette index", d.PaletteIndex]);
		}

		return rowList.ToArray();
	}
}
