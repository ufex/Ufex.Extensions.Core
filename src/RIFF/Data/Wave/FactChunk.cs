using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data.Wave;

/// <summary>
/// fact - Fact chunk
/// </summary>
internal class FactChunk : Chunk
{
	public UInt32 SampleLength { get; init; }

	public FactChunk(FileReader fr) : base(fr)
	{
		SampleLength = fr.ReadUInt32();
	}
}