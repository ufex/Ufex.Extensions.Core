using Ufex.API;
using Ufex.Extensions.Core.RIFF.Data;

namespace Ufex.Extensions.Core.RIFF.Data.Wave;

internal class WaveRiffChunk : RiffChunk
{
	/// <summary>
	/// RIFF WAVE Chunk Types
	/// </summary>
	private static readonly Dictionary<string, Type> CHUNK_TYPES = new()
	{
		{ "fmt ", typeof(FmtChunk) },
		{ "fact", typeof(FactChunk) },
		{ "data", typeof(DataChunk) },
		{ "cue ", typeof(CueChunk) },
		{ "plst", typeof(PlstChunk) },
		{ "smpl", typeof(SmplChunk) },
		{ "inst", typeof(InstChunk) }
	};

	public override Dictionary<string, Type> ChunkTypes { get { return CHUNK_TYPES; } }

	public WaveRiffChunk(FileReader fr) : base(fr)
	{
	}

}