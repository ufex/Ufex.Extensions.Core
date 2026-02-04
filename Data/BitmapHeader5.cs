using Ufex.API;

namespace Ufex.FileTypes.BMP.Data;

/// <summary>
/// BITMAPV5HEADER - Bitmap Info Header v5 (124 bytes)
/// </summary>
internal class BitmapV5Header : BitmapV4Header
{
	/// <summary>
	/// Rendering intent for the bitmap
	/// </summary>
	public uint Intent { get; init; }

	/// <summary>
	/// Offset to the start of the profile data
	/// </summary>
	public uint ProfileData { get; init; }

	/// <summary>
	/// Size of embedded profile data
	/// </summary>
	public uint ProfileSize { get; init; }

	/// <summary>
	/// Reserved, should be zero
	/// </summary>
	public uint Reserved { get; init; }

	public BitmapV5Header(FileReader fr) : base(fr)
	{
		Intent = fr.ReadUInt32();
		ProfileData = fr.ReadUInt32();
		ProfileSize = fr.ReadUInt32();
		Reserved = fr.ReadUInt32();
	}
}
