using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.Wave;

namespace Ufex.Extensions.Core.RIFF.Structure.Wave;

/// <summary>
/// Tree node for the note chunk.
/// </summary>
class NoteChunkNode : ChunkNode
{
	public NoteChunkNode(NoteChunk chunk)
		: base(chunk, chunk.ChunkIDString, "Note", TreeViewIcon.Text)
	{
	}

	public override object[][] GetRows()
	{
		var d = (NoteChunk)_chunk;
		return [
			["CuePointID", d.CuePointID],
			["Text", d.Text, d.TextString]
		];
	}
}
