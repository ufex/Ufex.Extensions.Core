using Ufex.API;
using Ufex.Extensions.Core.RIFF.Data;

namespace Ufex.Extensions.Core.RIFF.Data.WebP;

internal class WebPRiffChunk : RiffChunk
{
	/// <summary>
	/// RIFF WEBP Chunk Types
	/// </summary>
	private static readonly Dictionary<string, Type> CHUNK_TYPES = new()
	{
		{ "VP8X", typeof(Vp8xChunk) },
		{ "VP8 ", typeof(Vp8Chunk) },
		{ "VP8L", typeof(Vp8lChunk) },
		{ "ANIM", typeof(AnimChunk) },
		{ "ANMF", typeof(AnmfChunk) },
		{ "ALPH", typeof(AlphChunk) },
		{ "ICCP", typeof(IccpChunk) },
		{ "EXIF", typeof(ExifChunk) },
		{ "XMP ", typeof(XmpChunk) }
	};

	public override Dictionary<string, Type> ChunkTypes { get { return CHUNK_TYPES; } }

	public WebPRiffChunk(FileReader fr) : base(fr)
	{
	}
}
