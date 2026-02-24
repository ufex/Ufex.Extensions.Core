using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data;

/// <summary>
/// LIST/INFO Chunk
/// </summary>
internal class InfoListChunk : ListChunk
{
	private static readonly Dictionary<string, Type> SUB_CHUNK_TYPES = new()
	{
		{ "IARL", typeof(ZStrChunk) }, // Archival Location
		{ "IART", typeof(ZStrChunk) }, // Artist
		{ "ICMS", typeof(ZStrChunk) }, // Commissioned
		{ "ICMT", typeof(ZStrChunk) }, // Comments
		{ "ICOP", typeof(ZStrChunk) }, // Copyright
		{ "ICRD", typeof(ZStrChunk) }, // Creation Date
		{ "ICRP", typeof(ZStrChunk) }, // Cropped
		{ "IDIM", typeof(ZStrChunk) }, // Dimensions
		{ "IDPI", typeof(ZStrChunk) }, // Dots Per Inch
		{ "IENG", typeof(ZStrChunk) }, // Engineer
		{ "IGNR", typeof(ZStrChunk) }, // Genre
		{ "IKEY", typeof(ZStrChunk) }, // Keywords
		{ "ILGT", typeof(ZStrChunk) }, // Lightness
		{ "IMED", typeof(ZStrChunk) }, // Medium
		{ "INAM", typeof(ZStrChunk) }, // Name
		{ "IPLT", typeof(ZStrChunk) }, // Palette Setting
		{ "IPRD", typeof(ZStrChunk) }, // Product
		{ "ISBJ", typeof(ZStrChunk) }, // Subject
		{ "ISFT", typeof(ZStrChunk) }, // Software
		{ "ISHP", typeof(ZStrChunk) }, // Sharpness
		{ "ISRC", typeof(ZStrChunk) }, // Source
		{ "ISRF", typeof(ZStrChunk) }, // Source Form
		{ "ITCH", typeof(ZStrChunk) }, // Technician
	};

	public override Dictionary<string, Type> SubChunkTypes { get { return SUB_CHUNK_TYPES; } }

	public InfoListChunk(FileReader fr) : base(fr)
	{
	}

}