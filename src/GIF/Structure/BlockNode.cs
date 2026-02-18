using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.API.Visual;

namespace Ufex.Extensions.Core.GIF.Structure;

/// <summary>
/// Base TreeNode class for GIF file blocks
/// </summary>
internal abstract class BlockNode : TreeNode
{
	/// <summary>
	/// File offset where this block starts
	/// </summary>
	public long Offset { get; protected set; }

	/// <summary>
	/// Description of this block
	/// </summary>
	public string Description { get; protected set; }

	public override Visual[] Visuals => [new DataGridVisual(TableData(), "Data")];

	protected BlockNode(string text, TreeViewIcon icon, long offset)
		: base(text, icon, icon)
	{
		Offset = offset;
		Description = text;
	}

	/// <summary>
	/// Build table data for the detail panel
	/// </summary>
	public DynamicTableData TableData()
	{
		var rows = GetRows();
		var td = new DynamicTableData(3, $"GIF.{GetType().Name}");
		td.SetColumn(0, "Field");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");

		foreach (var row in rows)
		{
			td.AddRow(row[0], row[1], row[2]);
		}

		return td;
	}

	/// <summary>
	/// Get rows for the data table
	/// Override in derived classes to provide block-specific data
	/// </summary>
	protected abstract List<object[]> GetRows();
}
