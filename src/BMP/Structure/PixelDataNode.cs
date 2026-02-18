using Ufex.API.Tree;
using Ufex.API.Format;
using Ufex.API.Tables;
using Ufex.API.Visual;

namespace Ufex.Extensions.Core.BMP.Structure;

/// <summary>
/// Pixel data node for BMP files
/// </summary>
internal class PixelDataNode : TreeNode
{
	private readonly long _offset;
	private readonly long _size;

	public override Visual[] Visuals => [new DataGridVisual(TableData(), "Pixel Data")];

	public PixelDataNode(long offset, long size)
		: base("Pixel Data", TreeViewIcon.Image, TreeViewIcon.Image)
	{
		_offset = offset;
		_size = size;
	}

	public DynamicTableData TableData()
	{
		var td = new DynamicTableData(3, "BMP.PixelData");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");

		td.AddRow("Offset", _offset, $"0x{_offset:X}");
		td.AddRow("Size", _size, ByteCountFormatter.Format((ulong)_size));

		return td;
	}
}
