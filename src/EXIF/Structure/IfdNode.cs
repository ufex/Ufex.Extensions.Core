using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Tree;
using Ufex.API.Visual;
using Ufex.Extensions.Core.EXIF.Data;

namespace Ufex.Extensions.Core.EXIF.Structure;

public class IfdNode : TreeNode
{
	private readonly Ifd _ifd;
	private readonly ExifData _exifData;

	public IfdNode(Ifd ifd, ExifData exifData, TreeNode[]? thumbnailNodes = null)
		: base(ifd.IfdType.ToString(), TreeViewIcon.Table, TreeViewIcon.Table)
	{
		_ifd = ifd;
		_exifData = exifData;

		Nodes.Add(new IfdHeaderNode(ifd));
		Nodes.Add(new IfdEntriesNode(ifd, exifData));

		if (thumbnailNodes != null)
		{
			foreach (var node in thumbnailNodes)
				Nodes.Add(node);
		}
	}

	public override Visual[] Visuals => [new DataGridVisual(BuildSummaryTable(), "Summary")];

	private DynamicTableData BuildSummaryTable()
	{
		var table = new DynamicTableData(2, $"EXIF.{_ifd.IfdType}.Summary");
		table.SetColumn(0, "Property");
		table.SetColumn(1, "Value");

		foreach (IfdEntry entry in _ifd.Entries)
		{
			string tagName = ExifConstants.GetTagName(_ifd.IfdType, entry.Tag);
			string value = entry.GetFormattedValue(_ifd.IfdType, _exifData.ByteOrder);
			table.AddRow(tagName, value);
		}

		return table;
	}
}
