namespace Ufex.FileTypes.BMP.Data;

/// <summary>
/// RGBTRIPLE - 3-byte color value (used in 24-bit bitmaps without alpha)
/// </summary>
internal struct RGBTRIPLE
{
	public byte Blue;
	public byte Green;
	public byte Red;

	public override readonly string ToString()
	{
		return $"#{Red:X2}{Green:X2}{Blue:X2}";
	}
}
