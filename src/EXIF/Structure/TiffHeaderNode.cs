using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Tree;
using Ufex.API.Visual;
using Ufex.Extensions.Core.EXIF.Data;

namespace Ufex.Extensions.Core.EXIF.Structure;

/// <summary>
/// Displays the TIFF header fields: byte order, magic number, and IFD0 offset.
/// </summary>
public class TiffHeaderNode : TreeNode
{
	private readonly TiffHeader _header;

	public TiffHeaderNode(TiffHeader header)
		: base("TIFF Header", TreeViewIcon.Header, TreeViewIcon.Header)
	{
		_header = header;
	}

	public override Visual[] Visuals => [new DataGridVisual(BuildTable(), "TIFF Header")];

	private DynamicTableData BuildTable()
	{
		var table = new DynamicTableData(4, "EXIF.TiffHeader");
		table.SetColumn(0, "Property");
		table.SetColumn(1, "Value");
		table.SetColumn(2, "Description");
		table.SetColumn(3, "Offset");

		long offset = _header.Offset;
		string byteOrderDesc = _header.ByteOrder == Endian.Little ? "Little-endian (Intel)" : "Big-endian (Motorola)";
		table.AddRow("Byte Order", _header.ByteOrder == Endian.Little ? (UInt16)0x4949 : (UInt16)0x4D4D, byteOrderDesc, new FileOffset(offset));
		offset += 2;
		table.AddRow("Magic", _header.Magic, $"0x{_header.Magic:X4}", new FileOffset(offset));
		offset += 2;
		table.AddRow("IFD0 Offset", _header.Ifd0Offset, $"0x{_header.Ifd0Offset:X8}", new FileOffset(offset));

		return table;
	}
}
