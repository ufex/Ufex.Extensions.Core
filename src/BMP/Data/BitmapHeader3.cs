using Ufex.API;

namespace Ufex.Extensions.Core.BMP.Data;

/// <summary>
/// BITMAPINFOHEADER - Bitmap Info Header v3 (40 bytes)
/// </summary>
internal class BitmapInfoHeader
{
	/// <summary>
	/// File offset where this header starts
	/// </summary>
	public long Offset { get; init; }

	/// <summary>
	/// Size of this header in bytes
	/// </summary>
	public uint Size { get; init; }

	/// <summary>
	/// Image width in pixels
	/// </summary>
	public int Width { get; init; }

	/// <summary>
	/// Image height in pixels (negative = top-down DIB)
	/// </summary>
	public int Height { get; init; }

	/// <summary>
	/// Number of color planes, must be 1
	/// </summary>
	public ushort Planes { get; init; }

	/// <summary>
	/// Number of bits per pixel (1, 4, 8, 16, 24, or 32)
	/// </summary>
	public ushort BitsPerPixel { get; init; }

	/// <summary>
	/// Compression method used
	/// </summary>
	public uint Compression { get; init; }

	/// <summary>
	/// Size of bitmap data in bytes
	/// </summary>
	public uint SizeOfBitmap { get; init; }

	/// <summary>
	/// Horizontal resolution in pixels per meter
	/// </summary>
	public int HorzResolution { get; init; }

	/// <summary>
	/// Vertical resolution in pixels per meter
	/// </summary>
	public int VertResolution { get; init; }

	/// <summary>
	/// Number of colors in the color table
	/// </summary>
	public uint ColorsUsed { get; init; }

	/// <summary>
	/// Number of important colors
	/// </summary>
	public uint ColorsImportant { get; init; }

	public BitmapInfoHeader()
	{
	}

	public BitmapInfoHeader(FileReader fr)
	{
		Offset = fr.BaseStream.Position;
		Size = fr.ReadUInt32();
		Width = fr.ReadInt32();
		Height = fr.ReadInt32();
		Planes = fr.ReadUInt16();
		BitsPerPixel = fr.ReadUInt16();
		Compression = fr.ReadUInt32();
		SizeOfBitmap = fr.ReadUInt32();
		HorzResolution = fr.ReadInt32();
		VertResolution = fr.ReadInt32();
		ColorsUsed = fr.ReadUInt32();
		ColorsImportant = fr.ReadUInt32();
	}
}
