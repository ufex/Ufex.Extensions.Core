using Ufex.API;

namespace Ufex.FileTypes.GIF.Data;

/// <summary>
/// Plain Text Extension (GIF89a only)
/// Renders text graphically on the image
/// </summary>
internal class PlainTextExtension : GifBlock
{
	/// <summary>
	/// Extension Introducer (always 0x21)
	/// </summary>
	public byte ExtensionIntroducer { get; init; }

	/// <summary>
	/// Plain Text Label (always 0x01)
	/// </summary>
	public byte PlainTextLabel { get; init; }

	/// <summary>
	/// Block Size (always 0x0C = 12)
	/// </summary>
	public byte BlockSize { get; init; }

	/// <summary>
	/// Text grid left position
	/// </summary>
	public ushort TextGridLeftPosition { get; init; }

	/// <summary>
	/// Text grid top position
	/// </summary>
	public ushort TextGridTopPosition { get; init; }

	/// <summary>
	/// Text grid width
	/// </summary>
	public ushort TextGridWidth { get; init; }

	/// <summary>
	/// Text grid height
	/// </summary>
	public ushort TextGridHeight { get; init; }

	/// <summary>
	/// Character cell width
	/// </summary>
	public byte CharacterCellWidth { get; init; }

	/// <summary>
	/// Character cell height
	/// </summary>
	public byte CharacterCellHeight { get; init; }

	/// <summary>
	/// Text foreground color index
	/// </summary>
	public byte TextForegroundColorIndex { get; init; }

	/// <summary>
	/// Text background color index
	/// </summary>
	public byte TextBackgroundColorIndex { get; init; }

	/// <summary>
	/// Plain text data sub-blocks
	/// </summary>
	public List<byte[]> DataBlocks { get; init; }

	/// <summary>
	/// Get the plain text content
	/// </summary>
	public string TextContent
	{
		get
		{
			var sb = new System.Text.StringBuilder();
			foreach (var block in DataBlocks)
			{
				sb.Append(System.Text.Encoding.ASCII.GetString(block));
			}
			return sb.ToString();
		}
	}

	public PlainTextExtension(FileReader fr) : base(fr)
	{
		ExtensionIntroducer = fr.ReadByte();
		PlainTextLabel = fr.ReadByte();
		BlockSize = fr.ReadByte();
		TextGridLeftPosition = fr.ReadUInt16();
		TextGridTopPosition = fr.ReadUInt16();
		TextGridWidth = fr.ReadUInt16();
		TextGridHeight = fr.ReadUInt16();
		CharacterCellWidth = fr.ReadByte();
		CharacterCellHeight = fr.ReadByte();
		TextForegroundColorIndex = fr.ReadByte();
		TextBackgroundColorIndex = fr.ReadByte();

		// Read data sub-blocks
		DataBlocks = new List<byte[]>();
		while (true)
		{
			byte blockSize = fr.ReadByte();
			if (blockSize == 0)
				break;
			DataBlocks.Add(fr.ReadBytes(blockSize));
		}
	}
}
