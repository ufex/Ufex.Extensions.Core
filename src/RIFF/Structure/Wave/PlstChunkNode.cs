using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.Wave;

namespace Ufex.Extensions.Core.RIFF.Structure.Wave;

/// <summary>
/// Tree node for the playlist chunk.
/// </summary>
class PlstChunkNode : ChunkNode
{
	public PlstChunkNode(PlstChunk chunk)
		: base(chunk, chunk.ChunkIDString, "Playlist", TreeViewIcon.List)
	{
	}

	public override object[][] GetRows()
	{
		var d = (PlstChunk)_chunk;
		var rows = new List<object[]>([
			["SegmentCount", d.SegmentCount]
		]);
		for (int i = 0; i < d.Segments.Length; i++)
		{
			var seg = d.Segments[i];
			rows.Add([$"Segment[{i}].CuePointID", seg.CuePointID]);
			rows.Add([$"Segment[{i}].Length", seg.Length, $"{seg.Length} samples"]);
			rows.Add([$"Segment[{i}].Repeats", seg.Repeats, seg.Repeats == 0 ? "Infinite" : $"{seg.Repeats}"]);
		}
		return rows.ToArray();
	}
}
