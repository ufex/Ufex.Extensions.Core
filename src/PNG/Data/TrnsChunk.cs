using Ufex.API;

namespace Ufex.Extensions.Core.PNG.Data;

/// <summary>
/// tRNS - Transparency
/// </summary>
internal class TrnsChunk : Chunk
{
	public int ColorType { get; init; }

	// For color type 0
	public ushort GreySampleValue { get; init; }

	// For color type 2
	public ushort RedSampleValue { get; init; }
	public ushort GreenSampleValue { get; init; }
	public ushort BlueSampleValue { get; init; }

	// For color type 3
	public byte[] PaletteAlpha { get; init; } = Array.Empty<byte>();

	public TrnsChunk(FileReader fr, int colorType) : base(fr)
	{
		if (colorType == 0 || (colorType == -1 && Length == 2))
		{
			ColorType = 0;
			GreySampleValue = fr.ReadUInt16();
		}
		else if (colorType == 2 || (colorType == -1 && Length == 6))
		{
			ColorType = 2;
			RedSampleValue = fr.ReadUInt16();
			GreenSampleValue = fr.ReadUInt16();
			BlueSampleValue = fr.ReadUInt16();
		}
		else if (colorType == 3)
		{
			ColorType = 3;
			PaletteAlpha = new byte[Length];
			for (int i = 0; i < PaletteAlpha.Length; i++)
			{
				PaletteAlpha[i] = fr.ReadByte();
			}
		}
	}
}
