using Ufex.API;

namespace Ufex.FileTypes.GIF.Data;

/// <summary>
/// RGB color entry (3 bytes)
/// </summary>
internal struct RgbColor
{
	public byte Red;
	public byte Green;
	public byte Blue;

	public RgbColor(FileReader fr)
	{
		Red = fr.ReadByte();
		Green = fr.ReadByte();
		Blue = fr.ReadByte();
	}

	public override readonly string ToString()
	{
		return $"#{Red:X2}{Green:X2}{Blue:X2}";
	}
}

/// <summary>
/// Color Table - Global or Local
/// Contains an array of RGB color entries
/// </summary>
internal class ColorTable : GifBlock
{
	/// <summary>
	/// Size of the color table in bytes
	/// </summary>
	public int Size { get; init; }

	/// <summary>
	/// Number of colors in the table
	/// </summary>
	public int ColorCount { get; init; }

	/// <summary>
	/// Array of RGB colors
	/// </summary>
	public RgbColor[] Colors { get; init; }

	public ColorTable(FileReader fr, int sizeOfColorTable) : base(fr)
	{
		ColorCount = 1 << (sizeOfColorTable + 1);
		Size = ColorCount * 3;

		Colors = new RgbColor[ColorCount];
		for (int i = 0; i < ColorCount; i++)
		{
			Colors[i] = new RgbColor(fr);
		}
	}
}
