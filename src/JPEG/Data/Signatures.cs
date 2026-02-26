namespace Ufex.Extensions.Core.JPEG.Data;

/// <summary>
/// JPEG marker signatures and magic numbers
/// </summary>
internal static class Signatures
{
	/// <summary>
	/// JPEG files always start with SOI marker: FF D8
	/// </summary>
	public static readonly byte[] FileSignature = [0xFF, 0xD8];

	/// <summary>
	/// JFIF APP0 identifier string: "JFIF\0"
	/// </summary>
	public static readonly byte[] JfifIdentifier = [0x4A, 0x46, 0x49, 0x46, 0x00];

	/// <summary>
	/// JFXX APP0 identifier string: "JFXX\0"
	/// </summary>
	public static readonly byte[] JfxxIdentifier = [0x4A, 0x46, 0x58, 0x58, 0x00];

	// Marker prefix
	public const byte MarkerPrefix = 0xFF;

	// Standalone markers (no length field)
	public const byte SOI = 0xD8;  // Start of Image
	public const byte EOI = 0xD9;  // End of Image
	public const byte RST0 = 0xD0; // Restart marker 0
	public const byte RST7 = 0xD7; // Restart marker 7

	// Markers with length field
	public const byte APP0 = 0xE0;  // Application segment 0 (JFIF/JFXX)
	public const byte APP1 = 0xE1;  // Application segment 1 (Exif)
	public const byte APP2 = 0xE2;  // Application segment 2
	public const byte APP15 = 0xEF; // Application segment 15

	public const byte SOF0 = 0xC0;  // Start of Frame - Baseline DCT
	public const byte SOF1 = 0xC1;  // Start of Frame - Extended Sequential DCT
	public const byte SOF2 = 0xC2;  // Start of Frame - Progressive DCT
	public const byte SOF3 = 0xC3;  // Start of Frame - Lossless (Sequential)

	public const byte DHT = 0xC4;   // Define Huffman Table
	public const byte DQT = 0xDB;   // Define Quantization Table
	public const byte DRI = 0xDD;   // Define Restart Interval
	public const byte SOS = 0xDA;   // Start of Scan
	public const byte COM = 0xFE;   // Comment
}
