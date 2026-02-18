using Ufex.API;

namespace Ufex.Extensions.Core.PNG.Data;

/// <summary>
/// cHRM - Primary chromaticities and white point
/// </summary>
internal class ChrmChunk : Chunk
{
	public uint WhitePointX { get; init; }
	public uint WhitePointY { get; init; }
	public uint RedX { get; init; }
	public uint RedY { get; init; }
	public uint GreenX { get; init; }
	public uint GreenY { get; init; }
	public uint BlueX { get; init; }
	public uint BlueY { get; init; }

	public ChrmChunk(FileReader fr) : base(fr)
	{
		WhitePointX = fr.ReadUInt32();
		WhitePointY = fr.ReadUInt32();
		RedX = fr.ReadUInt32();
		RedY = fr.ReadUInt32();
		GreenX = fr.ReadUInt32();
		GreenY = fr.ReadUInt32();
		BlueX = fr.ReadUInt32();
		BlueY = fr.ReadUInt32();
	}
}
