using Ufex.API;

namespace Ufex.FileTypes.PNG.Data;

/// <summary>
/// tIME - Image last-modification time
/// </summary>
internal class TimeChunk : Chunk
{
	public ushort Year { get; init; }
	public byte Month { get; init; }
	public byte Day { get; init; }
	public byte Hour { get; init; }
	public byte Minute { get; init; }
	public byte Second { get; init; }

	public TimeChunk(FileReader fr) : base(fr)
	{
		Year = fr.ReadUInt16();
		Month = fr.ReadByte();
		Day = fr.ReadByte();
		Hour = fr.ReadByte();
		Minute = fr.ReadByte();
		Second = fr.ReadByte();
	}
}
