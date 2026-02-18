using Ufex.API;

namespace Ufex.Extensions.Core.BMP.Data;

/// <summary>
/// ICONDIRENTRY - Icon directory entry
/// </summary>
internal class IconDirEntry
{
	/// <summary>
	/// File offset where this entry starts
	/// </summary>
	public long Offset { get; init; }

	/// <summary>
	/// Width in pixels (0 = 256 pixels)
	/// </summary>
	public byte Width { get; init; }

	/// <summary>
	/// Height in pixels (0 = 256 pixels)
	/// </summary>
	public byte Height { get; init; }

	/// <summary>
	/// Number of colors in palette (0 if more than 256)
	/// </summary>
	public byte ColorCount { get; init; }

	/// <summary>
	/// Reserved, should be zero
	/// </summary>
	public byte Reserved { get; init; }

	/// <summary>
	/// Color planes (ICO) or hotspot X (CUR)
	/// </summary>
	public ushort Planes { get; init; }

	/// <summary>
	/// Bits per pixel (ICO) or hotspot Y (CUR)
	/// </summary>
	public ushort BitCount { get; init; }

	/// <summary>
	/// Size of image data in bytes
	/// </summary>
	public uint BytesInRes { get; init; }

	/// <summary>
	/// Offset to image data from beginning of file
	/// </summary>
	public uint ImageOffset { get; init; }

	/// <summary>
	/// Actual width (handles 0 = 256 case)
	/// </summary>
	public int ActualWidth => Width == 0 ? 256 : Width;

	/// <summary>
	/// Actual height (handles 0 = 256 case)
	/// </summary>
	public int ActualHeight => Height == 0 ? 256 : Height;

	public IconDirEntry(FileReader fr)
	{
		Offset = fr.BaseStream.Position;
		Width = fr.ReadByte();
		Height = fr.ReadByte();
		ColorCount = fr.ReadByte();
		Reserved = fr.ReadByte();
		Planes = fr.ReadUInt16();
		BitCount = fr.ReadUInt16();
		BytesInRes = fr.ReadUInt32();
		ImageOffset = fr.ReadUInt32();
	}
}
