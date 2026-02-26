using Ufex.API;

namespace Ufex.Extensions.Core.JPEG.Data;

/// <summary>
/// APP0 JFXX marker segment (0xFFE0 with "JFXX\0" identifier)
/// Optional JFIF extension for thumbnails in formats other than 24-bit RGB.
/// Must immediately follow the JFIF APP0 marker segment.
/// </summary>
internal class App0JfxxSegment : Segment
{
	/// <summary>
	/// Identifier string bytes: "JFXX\0" (5 bytes)
	/// </summary>
	public byte[] Identifier { get; init; }

	/// <summary>
	/// Extension code:
	/// 0x10 = Thumbnail coded using JPEG
	/// 0x11 = Thumbnail stored using 1 byte/pixel
	/// 0x13 = Thumbnail stored using 3 bytes/pixel
	/// </summary>
	public byte ExtensionCode { get; init; }

	/// <summary>
	/// Extension data (variable length, depends on extension code)
	/// </summary>
	public byte[] ExtensionData { get; init; }

	public App0JfxxSegment(FileReader fr) : base(fr)
	{
		Identifier = fr.ReadBytes(5);
		ExtensionCode = fr.ReadByte();

		// Read remaining extension data
		// Length includes the 2-byte length field, so data bytes = Length - 2 - 5 (identifier) - 1 (extension code)
		int dataLength = Length - 2 - 5 - 1;
		ExtensionData = dataLength > 0 ? fr.ReadBytes(dataLength) : [];
	}
}
