using Ufex.API;
using Ufex.API.Tree;
using Ufex.Extensions.Core.PNG.Data;

namespace Ufex.Extensions.Core.PNG.Structure;

/// <summary>
/// iTXt - International textual data
/// </summary>
class ItxtChunkNode : ChunkNode
{
	public ItxtChunkNode(ItxtChunk chunk)
		: base(chunk, "iTXt", "International textual data", TreeViewIcon.Section)
	{
	}

	public override object[][] GetRows()
	{
		var d = (ItxtChunk)Chunk;
		object[][] rows = [
			["Keyword", d.Keyword],
			["Language Tag", d.LanguageTag],
			["Translated Keyword", d.TranslatedKeyword],
			["Compression Flag", d.CompressionFlag, d.IsCompressed ? "Compressed" : "Uncompressed"],
			["Compression Method", d.CompressionMethod, d.CompressionMethod == 0 ? "Deflate" : "Unknown"],
			["Text", d.Text],
		];
		return rows;
	}
}
