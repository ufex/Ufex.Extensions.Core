namespace Ufex.Extensions.Core.JPEG.Data;

/// <summary>
/// JPEG/JFIF constant lookup tables
/// </summary>
internal static class Constants
{
	/// <summary>
	/// JFIF density unit names
	/// </summary>
	public static readonly Dictionary<byte, string> DensityUnits = new()
	{
		{ 0, "No units (aspect ratio only)" },
		{ 1, "Dots per inch" },
		{ 2, "Dots per centimetre" },
	};

	/// <summary>
	/// SOF compression type names
	/// </summary>
	public static readonly Dictionary<byte, string> SofTypes = new()
	{
		{ Signatures.SOF0, "Baseline DCT" },
		{ Signatures.SOF1, "Extended Sequential DCT" },
		{ Signatures.SOF2, "Progressive DCT" },
		{ Signatures.SOF3, "Lossless (Sequential)" },
	};

	/// <summary>
	/// JFXX thumbnail extension codes
	/// </summary>
	public static readonly Dictionary<byte, string> JfxxExtensionCodes = new()
	{
		{ 0x10, "Thumbnail coded using JPEG" },
		{ 0x11, "Thumbnail stored using 1 byte/pixel" },
		{ 0x13, "Thumbnail stored using 3 bytes/pixel" },
	};

	/// <summary>
	/// Huffman table class names
	/// </summary>
	public static readonly Dictionary<int, string> HuffmanTableClasses = new()
	{
		{ 0, "DC" },
		{ 1, "AC" },
	};

	/// <summary>
	/// JPEG marker type display names
	/// </summary>
	public static readonly Dictionary<byte, string> MarkerNames = new()
	{
		{ Signatures.SOI, "SOI" },
		{ Signatures.EOI, "EOI" },
		{ Signatures.APP0, "APP0" },
		{ Signatures.APP1, "APP1" },
		{ Signatures.SOF0, "SOF0" },
		{ Signatures.SOF1, "SOF1" },
		{ Signatures.SOF2, "SOF2" },
		{ Signatures.SOF3, "SOF3" },
		{ Signatures.DHT, "DHT" },
		{ Signatures.DQT, "DQT" },
		{ Signatures.DRI, "DRI" },
		{ Signatures.SOS, "SOS" },
		{ Signatures.COM, "COM" },
	};

	/// <summary>
	/// Gets the display name for a JPEG marker type
	/// </summary>
	public static string GetMarkerName(byte markerType)
	{
		if (MarkerNames.TryGetValue(markerType, out var name))
			return name;

		// APPn markers
		if (markerType >= 0xE0 && markerType <= 0xEF)
			return $"APP{markerType - 0xE0}";

		// RSTn markers
		if (markerType >= Signatures.RST0 && markerType <= Signatures.RST7)
			return $"RST{markerType - Signatures.RST0}";

		// SOFn markers
		if (markerType >= 0xC0 && markerType <= 0xCF && markerType != 0xC4 && markerType != 0xC8 && markerType != 0xCC)
			return $"SOF{markerType - 0xC0}";

		return $"0x{markerType:X2}";
	}

	/// <summary>
	/// Gets the density unit display string
	/// </summary>
	public static string GetDensityUnit(byte units)
	{
		return DensityUnits.TryGetValue(units, out var name) ? name : "Unknown";
	}

	/// <summary>
	/// Gets the SOF type description
	/// </summary>
	public static string GetSofType(byte markerType)
	{
		return SofTypes.TryGetValue(markerType, out var name) ? name : "Unknown";
	}

	/// <summary>
	/// Returns true if the marker type is a standalone marker (no length/data)
	/// </summary>
	public static bool IsStandaloneMarker(byte markerType)
	{
		return markerType == Signatures.SOI
			|| markerType == Signatures.EOI
			|| (markerType >= Signatures.RST0 && markerType <= Signatures.RST7);
	}
}
