using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.Wave;

namespace Ufex.Extensions.Core.RIFF.Structure.Wave;

/// <summary>
/// Tree node for the labeled text chunk.
/// </summary>
class LtxtChunkNode : ChunkNode
{
	public LtxtChunkNode(LtxtChunk chunk)
		: base(chunk, chunk.ChunkIDString, "Labeled text", TreeViewIcon.Text)
	{
	}

	public override object[][] GetRows()
	{
		var d = (LtxtChunk)_chunk;
		return [
			["CuePointID", d.CuePointID],
			["SampleLength", d.SampleLength, $"{d.SampleLength} samples"],
			["PurposeID", d.PurposeID, d.PurposeIDString],
			["Country", d.Country],
			["Language", d.Language],
			["Dialect", d.Dialect],
			["CodePage", d.CodePage],
			["Text", d.Text, d.TextString]
		];
	}
}
