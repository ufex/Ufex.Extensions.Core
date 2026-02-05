namespace Ufex.FileTypes.GIF.Data;

/// <summary>
/// GIF format constants
/// </summary>
internal static class Constants
{
	// Block types
	public const byte BLOCK_EXTENSION = 0x21;       // Extension Introducer
	public const byte BLOCK_IMAGE_DESCRIPTOR = 0x2C; // Image Descriptor
	public const byte BLOCK_TRAILER = 0x3B;         // Trailer (end of file)

	// Extension labels
	public const byte EXT_GRAPHIC_CONTROL = 0xF9;   // Graphic Control Extension
	public const byte EXT_COMMENT = 0xFE;           // Comment Extension
	public const byte EXT_PLAIN_TEXT = 0x01;        // Plain Text Extension
	public const byte EXT_APPLICATION = 0xFF;       // Application Extension

	// Disposal methods
	public static readonly Dictionary<int, string> DisposalMethods = new()
	{
		{ 0, "No disposal specified" },
		{ 1, "Do not dispose" },
		{ 2, "Restore to background color" },
		{ 3, "Restore to previous" },
		{ 4, "Reserved" },
		{ 5, "Reserved" },
		{ 6, "Reserved" },
		{ 7, "Reserved" }
	};

	/// <summary>
	/// Get display string for disposal method
	/// </summary>
	public static string GetDisposalMethod(int method)
	{
		return DisposalMethods.TryGetValue(method, out var desc) ? desc : "Unknown";
	}
}
