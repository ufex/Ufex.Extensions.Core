using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data;

internal class InfoListChunk : ListChunk
{
	private static readonly Dictionary<string, Type> SUB_CHUNK_TYPES = new()
	{
		{ "IARL", typeof(ZStrChunk) },
		{ "IART", typeof(ZStrChunk) },
		{ "ICMS", typeof(ZStrChunk) },
		{ "ICMT", typeof(ZStrChunk) },
		{ "ISFT", typeof(ZStrChunk) },
		{ "ISRC", typeof(ZStrChunk) },
		{ "ISRF", typeof(ZStrChunk) },
		{ "ICRD", typeof(ZStrChunk) }
		// TODO: complete the list
	};

	public override Dictionary<string, Type> SubChunkTypes { get { return SUB_CHUNK_TYPES; } }

	public InfoListChunk(FileReader fr) : base(fr)
	{
	}

}