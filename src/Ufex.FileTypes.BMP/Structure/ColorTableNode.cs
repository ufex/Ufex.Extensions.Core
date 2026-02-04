using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.FileTypes.BMP.Data;

namespace Ufex.FileTypes.BMP.Structure;

/// <summary>
/// Color table node for BMP files
/// </summary>
internal class ColorTableNode : TreeNode
{
	private readonly ColorTable _colorTable;

	public override Visual[] Visuals => [new DataGridVisual(TableData(), "Colors")];

	public ColorTableNode(ColorTable colorTable)
		: base("Color Table", TreeViewIcon.Palette, TreeViewIcon.Palette)
	{
		_colorTable = colorTable;
	}

	public DynamicTableData TableData()
	{
		var td = new DynamicTableData(6, "BMP.ColorTable");
		td.SetColumn(0, "Index");
		td.SetColumn(1, "Red");
		td.SetColumn(2, "Green");
		td.SetColumn(3, "Blue");
		td.SetColumn(4, "Reserved");
		td.SetColumn(5, "Color");

		for (int i = 0; i < _colorTable.Colors.Length; i++)
		{
			var color = _colorTable.Colors[i];
			td.AddRowData(new object[]
			{
				i,
				color.Red,
				color.Green,
				color.Blue,
				color.Reserved,
				color.ToString()
			});
		}

		return td;
	}
}
