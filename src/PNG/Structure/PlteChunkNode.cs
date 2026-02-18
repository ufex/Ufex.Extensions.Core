using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.Extensions.Core.PNG.Data;

namespace Ufex.Extensions.Core.PNG.Structure;

/// <summary>
/// PLTE - Palette
/// </summary>
class PlteChunkNode : ChunkNode
{
	public PlteChunkNode(PlteChunk chunk)
		: base(chunk, "PLTE", "Palette", TreeViewIcon.Section)
	{
	}

	public override DynamicTableData TableData()
	{
		var d = (PlteChunk)Chunk;
		DynamicTableData td = new DynamicTableData(3, "Png.RGB");
		td.SetColumn(0, "Red");
		td.SetColumn(1, "Green");
		td.SetColumn(2, "Blue");

		for (int i = 0; i < d.Palette.Length; i++)
		{
			td.AddRow(d.Palette[i].Red, d.Palette[i].Green, d.Palette[i].Blue);
		}
		return td;
	}
}
