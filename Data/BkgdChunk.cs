using Ufex.API;

namespace Ufex.FileTypes.PNG.Data;

/// <summary>
/// bKGD - Background colour
/// </summary>
internal class BkgdChunk : Chunk
{
	public int ColorType { get; init; }

	// For color type 0 & 4
	public ushort Greyscale { get; init; }

	// For color types 2 & 6
	public ushort Red { get; init; }
	public ushort Green { get; init; }
	public ushort Blue { get; init; }

	// For color type 3
	public byte PaletteIndex { get; init; }

	public BkgdChunk(FileReader fr, int colorType = -1) : base(fr)
	{
		ColorType = colorType;

		// Auto-detect color type based on length if not specified
		if (colorType == -1)
		{
			if (Length == 2)
			{
				ColorType = 0;
			}
			else if (Length == 6)
			{
				ColorType = 2;
			}
			else if (Length == 1)
			{
				ColorType = 3;
			}
		}

		if (ColorType == 0 || ColorType == 4)
		{
			Greyscale = fr.ReadUInt16();
		}
		else if (ColorType == 2 || ColorType == 6)
		{
			Red = fr.ReadUInt16();
			Green = fr.ReadUInt16();
			Blue = fr.ReadUInt16();
		}
		else if (ColorType == 3)
		{
			PaletteIndex = fr.ReadByte();
		}
	}
}
