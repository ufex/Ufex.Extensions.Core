using Ufex.API;
using Ufex.API.Tree;
using Ufex.FileTypes.PNG.Data;

namespace Ufex.FileTypes.PNG.Structure;

/// <summary>
/// tEXt - Textual data
/// </summary>
class TextChunkNode : ChunkNode
{
	public TextChunkNode(TextChunk chunk)
		: base(chunk, "tEXt", "Textual data", TreeViewIcon.Section)
	{
	}

	public override object[][] GetRows()
	{
		var d = (TextChunk)Chunk;
		object[][] rows = [
			["Keyword", d.Keyword, d.KeywordString],
			["Null Separator", d.NullSeparator],
			["Text String", d.TextString, d.TextStringString],
		];
		return rows;
	}
}
