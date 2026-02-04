using Ufex.API;

namespace Ufex.FileTypes.BMP.Data;

/// <summary>
/// Icon image data (DIB format)
/// </summary>
internal class IconImage
{
	/// <summary>
	/// File offset where this image starts
	/// </summary>
	public long Offset { get; init; }

	/// <summary>
	/// Index of this image in the icon file
	/// </summary>
	public int Index { get; init; }

	/// <summary>
	/// DIB header for the icon image
	/// </summary>
	public BitmapInfoHeader Header { get; init; }

	/// <summary>
	/// Color table for indexed images
	/// </summary>
	public RGBQUAD[] Colors { get; init; }

	/// <summary>
	/// XOR mask data (the actual image pixels)
	/// </summary>
	public byte[] XorMask { get; init; }

	/// <summary>
	/// AND mask data (transparency mask)
	/// </summary>
	public byte[] AndMask { get; init; }

	public IconImage(FileReader fr, int index, IconDirEntry entry)
	{
		Index = index;
		Offset = fr.BaseStream.Position;

		// Read the DIB header
		Header = new BitmapInfoHeader(fr);

		// Calculate number of colors
		uint numColors = GetColorCount(Header.BitsPerPixel, Header.ColorsUsed);

		// Read color table
		Colors = new RGBQUAD[numColors];
		for (int i = 0; i < numColors; i++)
		{
			Colors[i] = new RGBQUAD(fr);
		}

		// The height in the header is doubled (XOR + AND masks)
		int actualHeight = Math.Abs(Header.Height) / 2;
		int width = Header.Width;

		// Calculate XOR mask size
		int xorRowSize = ((width * Header.BitsPerPixel + 31) / 32) * 4;
		int xorSize = xorRowSize * actualHeight;

		// Calculate AND mask size (1-bit mask)
		int andRowSize = ((width + 31) / 32) * 4;
		int andSize = andRowSize * actualHeight;

		// Read XOR mask
		XorMask = fr.ReadBytes(xorSize);

		// Read AND mask
		AndMask = fr.ReadBytes(andSize);
	}

	private static uint GetColorCount(ushort bitsPerPixel, uint colorsUsed)
	{
		if (colorsUsed > 0)
			return colorsUsed;

		return bitsPerPixel switch
		{
			1 => 2,
			4 => 16,
			8 => 256,
			_ => 0  // 16, 24, 32-bit images don't have a color table
		};
	}
}
