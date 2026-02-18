using Ufex.API;

namespace Ufex.Extensions.Core.PNG.Data;

/// <summary>
/// PLTE - Palette
/// </summary>
internal class PlteChunk : Chunk
{
	public RGB[] Palette { get; init; }

	public PlteChunk(FileReader fr) : base(fr)
	{
		uint numColors = Length / 3;
		Palette = new RGB[numColors];
		for (int i = 0; i < numColors; i++)
		{
			Palette[i] = new RGB
			{
				Red = fr.ReadByte(),
				Green = fr.ReadByte(),
				Blue = fr.ReadByte()
			};
		}
	}
}
