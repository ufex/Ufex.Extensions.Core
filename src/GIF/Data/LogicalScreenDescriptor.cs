using Ufex.API;

namespace Ufex.Extensions.Core.GIF.Data;

/// <summary>
/// Logical Screen Descriptor - 7 bytes following the header
/// Contains canvas dimensions and global color table information
/// </summary>
internal class LogicalScreenDescriptor : GifBlock
{
	/// <summary>
	/// Width of the logical screen in pixels
	/// </summary>
	public ushort Width { get; init; }

	/// <summary>
	/// Height of the logical screen in pixels
	/// </summary>
	public ushort Height { get; init; }

	/// <summary>
	/// Packed fields byte
	/// </summary>
	public byte PackedFields { get; init; }

	/// <summary>
	/// Global Color Table Flag (bit 7)
	/// If set, a Global Color Table follows
	/// </summary>
	public bool GlobalColorTableFlag { get; init; }

	/// <summary>
	/// Color Resolution (bits 4-6)
	/// Number of bits per primary color minus 1
	/// </summary>
	public int ColorResolution { get; init; }

	/// <summary>
	/// Sort Flag (bit 3)
	/// If set, Global Color Table is sorted by decreasing importance
	/// </summary>
	public bool SortFlag { get; init; }

	/// <summary>
	/// Size of Global Color Table (bits 0-2)
	/// Number of entries = 2^(SizeOfGlobalColorTable + 1)
	/// </summary>
	public int SizeOfGlobalColorTable { get; init; }

	/// <summary>
	/// Background Color Index
	/// Index into the Global Color Table for background color
	/// </summary>
	public byte BackgroundColorIndex { get; init; }

	/// <summary>
	/// Pixel Aspect Ratio
	/// If non-zero: Aspect Ratio = (PixelAspectRatio + 15) / 64
	/// </summary>
	public byte PixelAspectRatio { get; init; }

	/// <summary>
	/// Calculated number of colors in the Global Color Table
	/// </summary>
	public int GlobalColorTableSize => GlobalColorTableFlag ? (1 << (SizeOfGlobalColorTable + 1)) : 0;

	public LogicalScreenDescriptor(FileReader fr) : base(fr)
	{
		Width = fr.ReadUInt16();
		Height = fr.ReadUInt16();

		PackedFields = fr.ReadByte();

		// Unpack the packed fields
		GlobalColorTableFlag = (PackedFields & 0x80) != 0;        // Bit 7
		ColorResolution = ((PackedFields >> 4) & 0x07) + 1;       // Bits 4-6 + 1
		SortFlag = (PackedFields & 0x08) != 0;                    // Bit 3
		SizeOfGlobalColorTable = PackedFields & 0x07;             // Bits 0-2

		BackgroundColorIndex = fr.ReadByte();
		PixelAspectRatio = fr.ReadByte();
	}
}
