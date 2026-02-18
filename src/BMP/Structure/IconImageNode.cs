using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.Extensions.Core.BMP.Data;

namespace Ufex.Extensions.Core.BMP.Structure;

/// <summary>
/// Icon image node (contains header, colors, XOR and AND masks)
/// </summary>
internal class IconImageNode : TreeNode
{
	private readonly IconImage _image;

	public override Visual[] Visuals => [];

	public IconImageNode(IconImage image)
		: base($"Image [{image.Index}]", TreeViewIcon.Image, TreeViewIcon.Image)
	{
		_image = image;

		// Add child nodes
		Nodes.Add(new IconImageHeaderNode(_image));
		if (_image.Colors.Length > 0)
		{
			Nodes.Add(new IconImageColorsNode(_image));
		}
		Nodes.Add(new IconImageMaskNode("XOR Mask", _image.XorMask));
		Nodes.Add(new IconImageMaskNode("AND Mask", _image.AndMask));
	}
}

/// <summary>
/// Icon image DIB header node
/// </summary>
internal class IconImageHeaderNode : HeaderNode
{
	private readonly IconImage _image;

	public IconImageHeaderNode(IconImage image)
		: base("Header", "DIB Header", image.Header.Offset, TreeViewIcon.Header)
	{
		_image = image;
	}

	protected override object[][] GetRows()
	{
		var h = _image.Header;
		return [
			["biSize", h.Size],
			["biWidth", h.Width, $"{h.Width} pixels"],
			["biHeight", h.Height, $"{Math.Abs(h.Height) / 2} pixels (XOR + AND)"],
			["biPlanes", h.Planes],
			["biBitCount", h.BitsPerPixel],
			["biCompression", h.Compression, Constants.GetCompressionDescription(h.Compression)],
			["biSizeImage", h.SizeOfBitmap],
			["biXPelsPerMeter", h.HorzResolution],
			["biYPelsPerMeter", h.VertResolution],
			["biClrUsed", h.ColorsUsed],
			["biClrImportant", h.ColorsImportant],
		];
	}
}

/// <summary>
/// Icon image color table node
/// </summary>
internal class IconImageColorsNode : TreeNode
{
	private readonly IconImage _image;

	public override Visual[] Visuals => [new DataGridVisual(TableData(), "Colors")];

	public IconImageColorsNode(IconImage image)
		: base("Colors", TreeViewIcon.Palette, TreeViewIcon.Palette)
	{
		_image = image;
	}

	public DynamicTableData TableData()
	{
		var td = new DynamicTableData(5, "ICO.ColorTable");
		td.SetColumn(0, "Index");
		td.SetColumn(1, "Red");
		td.SetColumn(2, "Green");
		td.SetColumn(3, "Blue");
		td.SetColumn(4, "Color");

		for (int i = 0; i < _image.Colors.Length; i++)
		{
			var color = _image.Colors[i];
			td.AddRow(i, color.Red, color.Green, color.Blue, color.ToString());
		}

		return td;
	}
}

/// <summary>
/// Icon image mask data node (XOR or AND)
/// </summary>
internal class IconImageMaskNode : TreeNode
{
	private readonly byte[] _data;

	public override Visual[] Visuals => [new DataGridVisual(TableData(), "Mask Data")];

	public IconImageMaskNode(string name, byte[] data)
		: base(name, TreeViewIcon.Binary, TreeViewIcon.Binary)
	{
		_data = data;
	}

	public DynamicTableData TableData()
	{
		var td = new DynamicTableData(2, "ICO.MaskData");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");

		td.AddRow("Size", $"{_data.Length} bytes");

		return td;
	}
}
