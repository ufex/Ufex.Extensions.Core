using Ufex.API;

namespace Ufex.FileTypes.PNG.Data;

/// <summary>
/// sPLT - Suggested palette
/// </summary>
internal class SpltChunk : Chunk
{
	public string PaletteName { get; init; }
	public byte SampleDepth { get; init; }

	// If SampleDepth == 8
	public SugPalEntry8[]? Palette8 { get; init; }

	// If SampleDepth == 16
	public SugPalEntry16[]? Palette16 { get; init; }

	public SpltChunk(FileReader fr) : base(fr)
	{
		PaletteName = fr.ReadNullTerminatedString();
		SampleDepth = fr.ReadByte();

		int remainingBytes = (int)(Length - PaletteName.Length - 1 - 1);

		if (SampleDepth == 8)
		{
			int numEntries = remainingBytes / 6;
			var palette = new SugPalEntry8[numEntries];
			for (int i = 0; i < numEntries; i++)
			{
				palette[i] = new SugPalEntry8
				{
					Red = fr.ReadByte(),
					Green = fr.ReadByte(),
					Blue = fr.ReadByte(),
					Alpha = fr.ReadByte(),
					Frequency = fr.ReadUInt16()
				};
			}
			Palette8 = palette;
		}
		else if (SampleDepth == 16)
		{
			int numEntries = remainingBytes / 10;
			var palette = new SugPalEntry16[numEntries];
			for (int i = 0; i < numEntries; i++)
			{
				palette[i] = new SugPalEntry16
				{
					Red = fr.ReadUInt16(),
					Green = fr.ReadUInt16(),
					Blue = fr.ReadUInt16(),
					Alpha = fr.ReadUInt16(),
					Frequency = fr.ReadUInt16()
				};
			}
			Palette16 = palette;
		}
	}
}
