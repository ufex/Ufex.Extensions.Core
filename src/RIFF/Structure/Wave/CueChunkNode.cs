using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.Wave;

namespace Ufex.Extensions.Core.RIFF.Structure.Wave;

/// <summary>
/// Tree node for the cue chunk.
/// </summary>
class CueChunkNode : ChunkNode
{
	public CueChunkNode(CueChunk chunk)
		: base(chunk, chunk.ChunkIDString, "Cue points", TreeViewIcon.List)
	{
	}

	public override object[][] GetRows()
	{
		var d = (CueChunk)_chunk;
		var rows = new List<object[]>([
			["CuePointCount", d.CuePointCount]
		]);
		for (int i = 0; i < d.CuePoints.Length; i++)
		{
			var cp = d.CuePoints[i];
			rows.Add([$"CuePoint[{i}].ID", cp.ID]);
			rows.Add([$"CuePoint[{i}].Position", cp.Position]);
			rows.Add([$"CuePoint[{i}].DataChunkID", cp.DataChunkID, cp.DataChunkIDString]);
			rows.Add([$"CuePoint[{i}].ChunkStart", cp.ChunkStart]);
			rows.Add([$"CuePoint[{i}].BlockStart", cp.BlockStart]);
			rows.Add([$"CuePoint[{i}].SampleOffset", cp.SampleOffset]);
		}
		return rows.ToArray();
	}
}
