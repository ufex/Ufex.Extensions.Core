using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.Wave;

namespace Ufex.Extensions.Core.RIFF.Structure.Wave;

/// <summary>
/// Tree node for the instrument chunk.
/// </summary>
class InstChunkNode : ChunkNode
{
	public InstChunkNode(InstChunk chunk)
		: base(chunk, chunk.ChunkIDString, "Instrument", TreeViewIcon.Header)
	{
	}

	public override object[][] GetRows()
	{
		var d = (InstChunk)_chunk;
		return [
			["UnshiftedNote", d.UnshiftedNote, $"MIDI note {d.UnshiftedNote}"],
			["FineTune", (byte)d.FineTune, $"{d.FineTune} cents"],
			["Gain", (byte)d.Gain, $"{d.Gain} dB"],
			["LowNote", d.LowNote, $"MIDI note {d.LowNote}"],
			["HighNote", d.HighNote, $"MIDI note {d.HighNote}"],
			["LowVelocity", d.LowVelocity],
			["HighVelocity", d.HighVelocity]
		];
	}
}
