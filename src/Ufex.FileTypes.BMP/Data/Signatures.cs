namespace Ufex.FileTypes.BMP.Data;

/// <summary>
/// File signatures and magic numbers for BMP and ICO formats
/// </summary>
internal static class Signatures
{
	/// <summary>
	/// BMP file signature ("BM" = 0x424D, stored little-endian as 0x4D42)
	/// </summary>
	public const ushort BMP_SIGNATURE = 0x4D42;

	/// <summary>
	/// ICO file type identifier
	/// </summary>
	public const ushort ICO_TYPE = 0x0001;

	/// <summary>
	/// CUR file type identifier
	/// </summary>
	public const ushort CUR_TYPE = 0x0002;
}
