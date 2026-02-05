using Ufex.API;

namespace Ufex.FileTypes.GIF.Data;

/// <summary>
/// Image Descriptor - 10 bytes
/// Describes an individual image within the GIF
/// </summary>
internal class ImageDescriptor : GifBlock
{
	/// <summary>
	/// Image Separator (always 0x2C)
	/// </summary>
	public byte ImageSeparator { get; init; }

	/// <summary>
	/// Left position of the image in the logical screen
	/// </summary>
	public ushort LeftPosition { get; init; }

	/// <summary>
	/// Top position of the image in the logical screen
	/// </summary>
	public ushort TopPosition { get; init; }

	/// <summary>
	/// Width of the image in pixels
	/// </summary>
	public ushort Width { get; init; }

	/// <summary>
	/// Height of the image in pixels
	/// </summary>
	public ushort Height { get; init; }

	/// <summary>
	/// Packed fields byte
	/// </summary>
	public byte PackedFields { get; init; }

	/// <summary>
	/// Local Color Table Flag (bit 7)
	/// If set, a Local Color Table follows
	/// </summary>
	public bool LocalColorTableFlag { get; init; }

	/// <summary>
	/// Interlace Flag (bit 6)
	/// If set, image is interlaced
	/// </summary>
	public bool InterlaceFlag { get; init; }

	/// <summary>
	/// Sort Flag (bit 5)
	/// If set, Local Color Table is sorted
	/// </summary>
	public bool SortFlag { get; init; }

	/// <summary>
	/// Reserved bits (bits 3-4)
	/// </summary>
	public int Reserved { get; init; }

	/// <summary>
	/// Size of Local Color Table (bits 0-2)
	/// Number of entries = 2^(SizeOfLocalColorTable + 1)
	/// </summary>
	public int SizeOfLocalColorTable { get; init; }

	/// <summary>
	/// Calculated number of colors in the Local Color Table
	/// </summary>
	public int LocalColorTableSize => LocalColorTableFlag ? (1 << (SizeOfLocalColorTable + 1)) : 0;

	public ImageDescriptor(FileReader fr) : base(fr)
	{
		ImageSeparator = fr.ReadByte();
		LeftPosition = fr.ReadUInt16();
		TopPosition = fr.ReadUInt16();
		Width = fr.ReadUInt16();
		Height = fr.ReadUInt16();

		PackedFields = fr.ReadByte();

		// Unpack the packed fields
		LocalColorTableFlag = (PackedFields & 0x80) != 0;         // Bit 7
		InterlaceFlag = (PackedFields & 0x40) != 0;               // Bit 6
		SortFlag = (PackedFields & 0x20) != 0;                    // Bit 5
		Reserved = (PackedFields >> 3) & 0x03;                    // Bits 3-4
		SizeOfLocalColorTable = PackedFields & 0x07;              // Bits 0-2
	}
}
