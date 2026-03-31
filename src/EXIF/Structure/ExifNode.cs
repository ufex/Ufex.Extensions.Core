using Ufex.API.Tables;
using Ufex.API.Tree;
using Ufex.API.Visual;
using Ufex.Extensions.Core.EXIF.Data;

namespace Ufex.Extensions.Core.EXIF.Structure;

public class ExifNode : TreeNode
{
	private readonly ExifData _exifData;

	public ExifNode(ExifData exifData)
		: base("EXIF", TreeViewIcon.Properties, TreeViewIcon.Properties)
	{
		_exifData = exifData;

		Nodes.Add(new IfdNode(_exifData.Ifd0, _exifData));
		if (_exifData.ExifIfd != null)
			Nodes.Add(new IfdNode(_exifData.ExifIfd, _exifData));
		if (_exifData.GpsIfd != null)
			Nodes.Add(new IfdNode(_exifData.GpsIfd, _exifData));
		if (_exifData.Ifd1 != null)
			Nodes.Add(new IfdNode(_exifData.Ifd1, _exifData));
	}

	public override Visual[] Visuals => [new DataGridVisual(BuildHeaderTable(), "Header")];

	private DynamicTableData BuildHeaderTable()
	{
		DynamicTableData table = new DynamicTableData(2, "EXIF.Header");
		table.SetColumn(0, "Property");
		table.SetColumn(1, "Value");

		table.AddRow("Byte Order", _exifData.TiffHeader.ByteOrder.ToString());
		table.AddRow("Magic", $"0x{_exifData.TiffHeader.Magic:X4}");
		table.AddRow("IFD0 Offset", $"0x{_exifData.TiffHeader.Ifd0Offset:X8}");
		table.AddRow("IFD0 Entries", _exifData.Ifd0.Entries.Count.ToString());
		table.AddRow("ExifIFD", _exifData.ExifIfd != null ? "Present" : "Not present");
		table.AddRow("GPSIFD", _exifData.GpsIfd != null ? "Present" : "Not present");
		table.AddRow("IFD1", _exifData.Ifd1 != null ? "Present" : "Not present");
		return table;
	}
}
