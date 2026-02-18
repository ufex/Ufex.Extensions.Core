using Ufex.API;

namespace Ufex.Extensions.Core.BMP.Data;

/// <summary>
/// Color table containing RGBQUAD entries
/// </summary>
internal class ColorTable
{
	/// <summary>
	/// File offset where the color table starts
	/// </summary>
	public long Offset { get; init; }

	/// <summary>
	/// Color table entries
	/// </summary>
	public RGBQUAD[] Colors { get; init; }

	/// <summary>
	/// Number of colors in the table
	/// </summary>
	public int Count => Colors.Length;

	/// <summary>
	/// Number of entries with non-zero reserved byte (should be 0)
	/// </summary>
	public int InvalidReservedCount { get; init; }

	public ColorTable(FileReader fr, uint numColors)
	{
		Offset = fr.BaseStream.Position;
		Colors = new RGBQUAD[numColors];
		int invalidCount = 0;

		for (int i = 0; i < numColors; i++)
		{
			Colors[i] = new RGBQUAD(fr);
			if (Colors[i].Reserved != 0)
				invalidCount++;
		}

		InvalidReservedCount = invalidCount;
	}
}
