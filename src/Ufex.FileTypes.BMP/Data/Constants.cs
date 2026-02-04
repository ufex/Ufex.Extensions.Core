namespace Ufex.FileTypes.BMP.Data;

/// <summary>
/// Constants and lookup tables for BMP format
/// </summary>
internal static class Constants
{
	// Bitmap info header sizes
	public const uint INFO_HEADER_V3_SIZE = 0x28;  // 40 bytes - BITMAPINFOHEADER
	public const uint INFO_HEADER_V4_SIZE = 0x6C;  // 108 bytes - BITMAPV4HEADER
	public const uint INFO_HEADER_V5_SIZE = 0x7C;  // 124 bytes - BITMAPV5HEADER

	// Compression methods (biCompression field)
	public const uint BI_RGB = 0;
	public const uint BI_RLE8 = 1;
	public const uint BI_RLE4 = 2;
	public const uint BI_BITFIELDS = 3;
	public const uint BI_JPEG = 4;
	public const uint BI_PNG = 5;
	public const uint BI_ALPHABITFIELDS = 6;

	/// <summary>
	/// Compression method descriptions
	/// </summary>
	public static readonly Dictionary<uint, string> COMPRESSION_METHODS = new()
	{
		{ BI_RGB, "Uncompressed" },
		{ BI_RLE8, "RLE 8-bit" },
		{ BI_RLE4, "RLE 4-bit" },
		{ BI_BITFIELDS, "Bit Fields" },
		{ BI_JPEG, "JPEG" },
		{ BI_PNG, "PNG" },
		{ BI_ALPHABITFIELDS, "Alpha Bit Fields" }
	};

	/// <summary>
	/// Color space type descriptions (bV4CSType / bV5CSType field)
	/// </summary>
	public static readonly Dictionary<uint, string> COLOR_SPACE_TYPES = new()
	{
		{ 0x00000000, "LCS_CALIBRATED_RGB" },
		{ 0x73524742, "LCS_sRGB" },           // 'sRGB'
		{ 0x57696E20, "LCS_WINDOWS_COLOR_SPACE" }, // 'Win '
		{ 0x4C494E4B, "PROFILE_LINKED" },     // 'LINK'
		{ 0x4D424544, "PROFILE_EMBEDDED" }    // 'MBED'
	};

	/// <summary>
	/// Rendering intent descriptions (bV5Intent field)
	/// </summary>
	public static readonly Dictionary<uint, string> RENDERING_INTENTS = new()
	{
		{ 1, "LCS_GM_BUSINESS (Saturation)" },
		{ 2, "LCS_GM_GRAPHICS (Relative Colorimetric)" },
		{ 4, "LCS_GM_IMAGES (Perceptual)" },
		{ 8, "LCS_GM_ABS_COLORIMETRIC (Absolute Colorimetric)" }
	};

	/// <summary>
	/// Get compression method description
	/// </summary>
	public static string GetCompressionDescription(uint compression)
	{
		return COMPRESSION_METHODS.TryGetValue(compression, out var desc) ? desc : "Unknown";
	}

	/// <summary>
	/// Get color space type description
	/// </summary>
	public static string GetColorSpaceDescription(uint csType)
	{
		return COLOR_SPACE_TYPES.TryGetValue(csType, out var desc) ? desc : $"0x{csType:X8}";
	}

	/// <summary>
	/// Get rendering intent description
	/// </summary>
	public static string GetRenderingIntentDescription(uint intent)
	{
		return RENDERING_INTENTS.TryGetValue(intent, out var desc) ? desc : "Unknown";
	}
}
