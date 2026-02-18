using Ufex.API.Tree;
using Ufex.API.Format;
using Ufex.Extensions.Core.BMP.Data;

namespace Ufex.Extensions.Core.BMP.Structure;

/// <summary>
/// ICONDIRENTRY - Icon directory entry node
/// </summary>
internal class IconDirEntryNode : HeaderNode
{
	private readonly IconDirEntry _entry;
	private readonly int _index;

	public IconDirEntryNode(IconDirEntry entry, int index)
		: base($"Entry [{index}]", $"ICONDIRENTRY - {entry.ActualWidth}x{entry.ActualHeight}", entry.Offset, TreeViewIcon.Image)
	{
		_entry = entry;
		_index = index;
	}

	protected override object[][] GetRows()
	{
		return [
			["bWidth", _entry.Width, _entry.Width == 0 ? "256 pixels" : $"{_entry.Width} pixels"],
			["bHeight", _entry.Height, _entry.Height == 0 ? "256 pixels" : $"{_entry.Height} pixels"],
			["bColorCount", _entry.ColorCount, GetColorCountDescription()],
			["bReserved", _entry.Reserved, _entry.Reserved == 0 ? "" : "Should be 0"],
			["wPlanes", _entry.Planes],
			["wBitCount", _entry.BitCount, GetBitCountDescription()],
			["dwBytesInRes", _entry.BytesInRes, ByteCountFormatter.Format(_entry.BytesInRes)],
			["dwImageOffset", _entry.ImageOffset, $"0x{_entry.ImageOffset:X}"],
		];
	}

	private string GetColorCountDescription()
	{
		if (_entry.ColorCount == 0)
			return "More than 256 colors";
		return $"{_entry.ColorCount} colors";
	}

	private string GetBitCountDescription()
	{
		return _entry.BitCount switch
		{
			1 => "Monochrome",
			4 => "16 colors",
			8 => "256 colors",
			16 => "High Color",
			24 => "True Color",
			32 => "True Color + Alpha",
			_ => ""
		};
	}
}
