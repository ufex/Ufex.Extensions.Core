using System.Text;
using Ufex.API;

namespace Ufex.FileTypes.GIF.Data;

/// <summary>
/// Application Extension (GIF89a only)
/// Used for application-specific data like NETSCAPE looping animation
/// </summary>
internal class ApplicationExtension : GifBlock
{
	/// <summary>
	/// Extension Introducer (always 0x21)
	/// </summary>
	public byte ExtensionIntroducer { get; init; }

	/// <summary>
	/// Application Extension Label (always 0xFF)
	/// </summary>
	public byte ExtensionLabel { get; init; }

	/// <summary>
	/// Block Size (always 0x0B = 11)
	/// </summary>
	public byte BlockSize { get; init; }

	/// <summary>
	/// Application Identifier (8 bytes ASCII)
	/// </summary>
	public byte[] ApplicationIdentifier { get; init; }

	/// <summary>
	/// Application Authentication Code (3 bytes)
	/// </summary>
	public byte[] AuthenticationCode { get; init; }

	/// <summary>
	/// Application data sub-blocks
	/// </summary>
	public List<byte[]> DataBlocks { get; init; }

	/// <summary>
	/// Get Application Identifier as string
	/// </summary>
	public string ApplicationIdentifierString => Encoding.ASCII.GetString(ApplicationIdentifier);

	/// <summary>
	/// Get Authentication Code as string
	/// </summary>
	public string AuthenticationCodeString => Encoding.ASCII.GetString(AuthenticationCode);

	/// <summary>
	/// Whether this is a NETSCAPE looping extension
	/// </summary>
	public bool IsNetscapeExtension => ApplicationIdentifierString == "NETSCAPE" && AuthenticationCodeString == "2.0";

	/// <summary>
	/// Get loop count for NETSCAPE extension (0 = infinite)
	/// </summary>
	public int? LoopCount
	{
		get
		{
			if (!IsNetscapeExtension || DataBlocks.Count == 0)
				return null;

			var data = DataBlocks[0];
			if (data.Length >= 3 && data[0] == 0x01)
			{
				return data[1] | (data[2] << 8);
			}
			return null;
		}
	}

	public ApplicationExtension(FileReader fr) : base(fr)
	{
		ExtensionIntroducer = fr.ReadByte();
		ExtensionLabel = fr.ReadByte();
		BlockSize = fr.ReadByte();
		ApplicationIdentifier = fr.ReadBytes(8);
		AuthenticationCode = fr.ReadBytes(3);

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
