using Ufex.API;
using Ufex.API.Tree;
using Ufex.Extensions.Core.PDF.Data;

namespace Ufex.Extensions.Core.PDF.Structure;

/// <summary>
/// Node for the cross-reference table section.
/// </summary>
internal class XRefTableNode : TreeNode
{
	private readonly List<XRefEntry> _entries;
	private readonly long _offset;
	private readonly bool _isStream;

	public override Ufex.API.Visual.Visual[] Visuals
	{
		get { return [new Ufex.API.Visual.DataGridVisual(TableData(), "XRef Table")]; }
	}

	public XRefTableNode(List<XRefEntry> entries, long offset, bool isStream)
		: base("Cross-Reference Table", TreeViewIcon.Table, TreeViewIcon.Table)
	{
		_entries = entries;
		_offset = offset;
		_isStream = isStream;
	}

	private Ufex.API.Tables.DynamicTableData TableData()
	{
		var td = new Ufex.API.Tables.DynamicTableData(5, "Zip.PropertyValueDescription");
		td.SetColumn(0, "Object #");
		td.SetColumn(1, "Generation");
		td.SetColumn(2, "Offset");
		td.SetColumn(3, "Type");
		td.SetColumn(4, "Status");

		foreach (var entry in _entries.OrderBy(e => e.ObjectNumber))
		{
			string typeDesc = entry.Type switch
			{
				0 => "Free",
				1 => "Uncompressed",
				2 => "Compressed",
				_ => $"Unknown ({entry.Type})"
			};

			string status = entry.InUse ? "In Use" : "Free";

			if (entry.Type == 2)
				td.AddRow(entry.ObjectNumber, entry.Generation, $"Stream obj {entry.Offset}, index {entry.StreamIndex}", typeDesc, status);
			else
				td.AddRow(entry.ObjectNumber, entry.Generation, entry.Offset, typeDesc, status);
		}

		return td;
	}
}
