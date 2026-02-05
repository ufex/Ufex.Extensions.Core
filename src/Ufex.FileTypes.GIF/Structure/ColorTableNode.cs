using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.FileTypes.GIF.Data;

namespace Ufex.FileTypes.GIF.Structure;

/// <summary>
/// Color Table node (Global or Local)
/// </summary>
internal class ColorTableNode : TreeNode
{
	private readonly ColorTable _colorTable;
	private readonly bool _isGlobal;

	public override Visual[] Visuals => [new DataGridVisual(TableData(), "Colors")];

	public ColorTableNode(ColorTable colorTable, bool isGlobal)
		: base(isGlobal ? "Global Color Table" : "Local Color Table", TreeViewIcon.Palette, TreeViewIcon.Palette)
	{
		_colorTable = colorTable;
		_isGlobal = isGlobal;
	}

	public DynamicTableData TableData()
	{
		var td = new DynamicTableData(4, "GIF.ColorTable");
		td.SetColumn(0, "Index");
		td.SetColumn(1, "Red");
		td.SetColumn(2, "Green");
		td.SetColumn(3, "Blue");

		for (int i = 0; i < _colorTable.Colors.Length; i++)
		{
			var color = _colorTable.Colors[i];
			td.AddRow(i, color.Red, color.Green, color.Blue);
		}

		return td;
	}
}
