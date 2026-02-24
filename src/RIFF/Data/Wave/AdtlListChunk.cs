using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data.Wave;

/// <summary>
/// LIST "adtl" - Associated Data List chunk. Contains labels, notes, 
/// and labeled text chunks that are associated with cue points.
/// </summary>
internal class AdtlListChunk : ListChunk
{
	private static readonly Dictionary<string, Type> SubChunkTypeMap = new()
	{
		{ "labl", typeof(LablChunk) },
		{ "note", typeof(NoteChunk) },
		{ "ltxt", typeof(LtxtChunk) }
	};

	public override Dictionary<string, Type> SubChunkTypes => SubChunkTypeMap;

	public AdtlListChunk(FileReader fr) : base(fr)
	{
	}
}
