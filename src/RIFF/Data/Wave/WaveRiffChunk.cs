using Ufex.API;
using Ufex.Extensions.Core.RIFF.Data;

namespace Ufex.Extensions.Core.RIFF.Data.Wave;

internal class WaveRiffChunk : RiffChunk
{
	private static readonly Dictionary<string, Type> CHUNK_TYPES = new()
	{
		{ "fmt ", typeof(FmtChunk) },
		// TODO: complete the list
	};

	public override Dictionary<string, Type> ChunkTypes { get { return CHUNK_TYPES; } }

	public WaveRiffChunk(FileReader fr) : base(fr)
	{
	}

}