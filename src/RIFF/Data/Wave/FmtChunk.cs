using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data.Wave;

internal class FmtChunk : Chunk
{
	const int FORMAT_PCM = 0x0001;

	public UInt16 FormatTag { get; init; }
	public UInt16 Channels { get; init; }
	public UInt32 SampleRate { get; init; }
	public UInt32 AvgBytesPerSec { get; init; }
	public UInt16 BlockAlign { get; init; }
	public UInt16? BitsPerSample { get; init; }

	public FmtChunk(FileReader fr) : base(fr)
	{
		FormatTag = fr.ReadUInt16();
		Channels = fr.ReadUInt16();
		SampleRate = fr.ReadUInt32();
		AvgBytesPerSec = fr.ReadUInt32();
		BlockAlign = fr.ReadUInt16();
		if(FormatTag == FORMAT_PCM)
		{
			BitsPerSample = fr.ReadUInt16();
		}
	}
}