using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.Extensions.Core.INI.Data;

namespace Ufex.Extensions.Core.INI.Structure;

/// <summary>
/// Base TreeNode class for INI file lines
/// </summary>
internal class LineNode : TreeNode
{
	public Line Line { get; }
	public string Description { get; protected set; }

	public override Visual[] Visuals => [new DataGridVisual(TableData(), "Data")];

	public LineNode(Line line, string text, string description, TreeViewIcon icon)
		: base(text, icon, icon)
	{
		Line = line;
		Description = description;
	}

	public virtual DynamicTableData TableData()
	{
		var td = new DynamicTableData(3, "INI.LineDetails");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");

		td.AddRow("Line Number", Line.LineNumber + 1, "1-based line number");
		td.AddRow("Offset", Line.Offset, $"Byte offset in file");
		td.AddRow("Length", Line.Length, "Length in bytes");
		td.AddRow("Type", Line.Type.ToString(), "Line classification");
		td.AddRow("Raw Text", Line.RawText, "Original line text");

		return td;
	}
}
