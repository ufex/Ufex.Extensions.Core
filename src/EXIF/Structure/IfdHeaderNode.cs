using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Tree;
using Ufex.API.Visual;
using Ufex.Extensions.Core.EXIF.Data;

namespace Ufex.Extensions.Core.EXIF.Structure;

/// <summary>
/// Displays the IFD header info: entry count and next IFD offset.
/// </summary>
public class IfdHeaderNode : TreeNode
{
	private readonly Ifd _ifd;

	public IfdHeaderNode(Ifd ifd)
		: base("Header", TreeViewIcon.Header, TreeViewIcon.Header)
	{
		_ifd = ifd;
	}

	public override Visual[] Visuals => [new DataGridVisual(BuildTable(), "Header")];

	private DynamicTableData BuildTable()
	{
		var table = new DynamicTableData(4, $"EXIF.{_ifd.IfdType}.Header");
		table.SetColumn(0, "Property");
		table.SetColumn(1, "Value");
		table.SetColumn(2, "Description");
		table.SetColumn(3, "Offset");

		long offset = _ifd.Offset;
		table.AddRow("Entry Count", (UInt16)_ifd.Entries.Count, $"{_ifd.Entries.Count} entries", new FileOffset(offset));
		offset += 2;

		long entriesSize = _ifd.Entries.Count * 12L;
		table.AddRow("Next IFD Offset", _ifd.NextIfdOffset, $"0x{_ifd.NextIfdOffset:X8}", new FileOffset(offset + entriesSize));

		return table;
	}
}
