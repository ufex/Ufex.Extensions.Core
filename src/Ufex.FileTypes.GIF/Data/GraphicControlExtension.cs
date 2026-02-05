using Ufex.API;

namespace Ufex.FileTypes.GIF.Data;

/// <summary>
/// Graphic Control Extension - 8 bytes (GIF89a only)
/// Controls animation timing and transparency
/// </summary>
internal class GraphicControlExtension : GifBlock
{
	/// <summary>
	/// Extension Introducer (always 0x21)
	/// </summary>
	public byte ExtensionIntroducer { get; init; }

	/// <summary>
	/// Graphic Control Label (always 0xF9)
	/// </summary>
	public byte GraphicControlLabel { get; init; }

	/// <summary>
	/// Block Size (always 0x04)
	/// </summary>
	public byte BlockSize { get; init; }

	/// <summary>
	/// Packed fields byte
	/// </summary>
	public byte PackedFields { get; init; }

	/// <summary>
	/// Reserved bits (bits 5-7)
	/// </summary>
	public int Reserved { get; init; }

	/// <summary>
	/// Disposal Method (bits 2-4)
	/// </summary>
	public int DisposalMethod { get; init; }

	/// <summary>
	/// User Input Flag (bit 1)
	/// If set, expect user input before continuing
	/// </summary>
	public bool UserInputFlag { get; init; }

	/// <summary>
	/// Transparent Color Flag (bit 0)
	/// If set, TransparentColorIndex is valid
	/// </summary>
	public bool TransparentColorFlag { get; init; }

	/// <summary>
	/// Delay Time in hundredths of a second
	/// </summary>
	public ushort DelayTime { get; init; }

	/// <summary>
	/// Index of transparent color in color table
	/// </summary>
	public byte TransparentColorIndex { get; init; }

	/// <summary>
	/// Block Terminator (always 0x00)
	/// </summary>
	public byte BlockTerminator { get; init; }

	/// <summary>
	/// Get disposal method description
	/// </summary>
	public string DisposalMethodDescription => Constants.GetDisposalMethod(DisposalMethod);

	public GraphicControlExtension(FileReader fr) : base(fr)
	{
		ExtensionIntroducer = fr.ReadByte();
		GraphicControlLabel = fr.ReadByte();
		BlockSize = fr.ReadByte();

		PackedFields = fr.ReadByte();

		// Unpack the packed fields
		Reserved = (PackedFields >> 5) & 0x07;                    // Bits 5-7
		DisposalMethod = (PackedFields >> 2) & 0x07;              // Bits 2-4
		UserInputFlag = (PackedFields & 0x02) != 0;               // Bit 1
		TransparentColorFlag = (PackedFields & 0x01) != 0;        // Bit 0

		DelayTime = fr.ReadUInt16();
		TransparentColorIndex = fr.ReadByte();
		BlockTerminator = fr.ReadByte();
	}
}
