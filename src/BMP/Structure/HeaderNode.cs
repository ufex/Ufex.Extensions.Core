using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.API.Visual;

namespace Ufex.Extensions.Core.BMP.Structure;

/// <summary>
/// Base TreeNode class for BMP file headers and sections
/// </summary>
internal abstract class HeaderNode : TreeNode
{
	/// <summary>
	/// File offset where this header starts
	/// </summary>
	public long Offset { get; protected set; }

	/// <summary>
	/// Description displayed in the detail panel
	/// </summary>
	public string Description { get; protected set; }

	/// <summary>
	/// Visuals displayed when this node is selected
	/// </summary>
	public override Visual[] Visuals => [new DataGridVisual(TableData(), "Data")];

	protected HeaderNode(string text, string description, long offset, TreeViewIcon icon)
		: base(text, icon, icon)
	{
		Description = description;
		Offset = offset;
	}

	/// <summary>
	/// Builds the table data for the detail panel
	/// </summary>
	public virtual DynamicTableData TableData()
	{
		var td = new DynamicTableData(4, "BMP.PropertyValueDescription");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");
		td.SetColumn(3, "Offset");

		long offset = Offset;
		foreach (var row in GetRows())
		{
			td.AddRow(row[0], row[1], row.Length > 2 ? row[2] : "", new FileOffset(offset));
			offset += GetValueSize(row[1]);
		}

		return td;
	}

	/// <summary>
	/// Override in subclasses to provide header-specific rows
	/// Each row is [Property, Value] or [Property, Value, Description]
	/// </summary>
	protected abstract object[][] GetRows();

	private static long GetValueSize(object value)
	{
		return value switch
		{
			byte => sizeof(byte),
			sbyte => sizeof(sbyte),
			short => sizeof(short),
			ushort => sizeof(ushort),
			int => sizeof(int),
			uint => sizeof(uint),
			long => sizeof(long),
			ulong => sizeof(ulong),
			float => sizeof(float),
			double => sizeof(double),
			byte[] arr => arr.Length,
			_ => 0
		};
	}
}
