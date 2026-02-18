namespace Ufex.Extensions.Core.GIF.Data;

/// <summary>
/// GIF file signatures and magic numbers
/// </summary>
internal static class Signatures
{
	/// <summary>
	/// GIF87a signature bytes
	/// </summary>
	public static readonly byte[] GIF87A = { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }; // "GIF87a"

	/// <summary>
	/// GIF89a signature bytes
	/// </summary>
	public static readonly byte[] GIF89A = { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }; // "GIF89a"

	/// <summary>
	/// GIF signature prefix (first 3 bytes)
	/// </summary>
	public static readonly byte[] GIF_PREFIX = { 0x47, 0x49, 0x46 }; // "GIF"
}
