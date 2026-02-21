namespace Ufex.Extensions.Core.PDF.Data;

/// <summary>
/// PDF file signatures and magic bytes
/// </summary>
internal static class Signatures
{
	/// <summary>
	/// PDF header magic bytes: %PDF-
	/// </summary>
	public static readonly byte[] HeaderMagic = [0x25, 0x50, 0x44, 0x46, 0x2D]; // %PDF-

	/// <summary>
	/// End-of-file marker: %%EOF
	/// </summary>
	public static readonly byte[] EofMarker = [0x25, 0x25, 0x45, 0x4F, 0x46]; // %%EOF
}
