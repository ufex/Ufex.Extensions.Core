using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Tree;
using Ufex.API.Visual;
using Ufex.Extensions.Core.EXIF.Data;

namespace Ufex.Extensions.Core.EXIF.Structure;

/// <summary>
/// Displays the technical view of IFD entries (tag, type, count, value, description, offset).
/// </summary>
public class IfdEntriesNode : TreeNode
{
	private readonly Ifd _ifd;
	private readonly ExifData _exifData;

	public IfdEntriesNode(Ifd ifd, ExifData exifData)
		: base("Entries", TreeViewIcon.Table, TreeViewIcon.Table)
	{
		_ifd = ifd;
		_exifData = exifData;
	}

	public override Visual[] Visuals => [new DataGridVisual(BuildTable(), "Entries")];

	private DynamicTableData BuildTable()
	{
		var table = new DynamicTableData(6, $"EXIF.{_ifd.IfdType}.Entries");
		table.SetColumn(0, "Tag");
		table.SetColumn(1, "Type");
		table.SetColumn(2, "Count");
		table.SetColumn(3, "Value");
		table.SetColumn(4, "Description");
		table.SetColumn(5, "Offset");

		foreach (IfdEntry entry in _ifd.Entries)
		{
			table.AddRowData([
				$"0x{entry.Tag:X4}",
				entry.GetTypeName(),
				entry.Count,
				entry.GetFormattedValue(_ifd.IfdType, _exifData.ByteOrder),
				ExifConstants.GetTagDescription(_ifd.IfdType, entry.Tag),
				new FileOffset(entry.Offset),
			]);
		}

		return table;
	}
}
