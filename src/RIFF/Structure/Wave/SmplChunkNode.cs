using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.Wave;

namespace Ufex.Extensions.Core.RIFF.Structure.Wave;

/// <summary>
/// Tree node for the sampler chunk.
/// </summary>
class SmplChunkNode : ChunkNode
{
	private static readonly Dictionary<UInt32, string> SmpteFormatNames = new()
	{
		{ 0, "No SMPTE offset" },
		{ 24, "24 fps" },
		{ 25, "25 fps" },
		{ 29, "29.97 fps (drop frame)" },
		{ 30, "30 fps" }
	};

	private static readonly Dictionary<UInt32, string> LoopTypeNames = new()
	{
		{ 0, "Forward" },
		{ 1, "Ping Pong" },
		{ 2, "Reverse" }
	};

	public SmplChunkNode(SmplChunk chunk)
		: base(chunk, chunk.ChunkIDString, "Sampler", TreeViewIcon.Header)
	{
	}

	public override object[][] GetRows()
	{
		var d = (SmplChunk)_chunk;
		var rows = new List<object[]>([
			["Manufacturer", d.Manufacturer],
			["Product", d.Product],
			["SamplePeriod", d.SamplePeriod, $"{d.SamplePeriod} ns ({1_000_000_000.0 / d.SamplePeriod:F1} Hz)"],
			["MidiUnityNote", d.MidiUnityNote, $"MIDI note {d.MidiUnityNote}"],
			["MidiPitchFraction", d.MidiPitchFraction],
			["SmpteFormat", d.SmpteFormat, SmpteFormatNames.GetValueOrDefault(d.SmpteFormat, "Unknown")],
			["SmpteOffset", d.SmpteOffset],
			["SampleLoopCount", d.SampleLoopCount],
			["SamplerDataSize", d.SamplerDataSize]
		]);
		for (int i = 0; i < d.SampleLoops.Length; i++)
		{
			var loop = d.SampleLoops[i];
			rows.Add([$"Loop[{i}].ID", loop.ID]);
			rows.Add([$"Loop[{i}].Type", loop.Type, LoopTypeNames.GetValueOrDefault(loop.Type, "Unknown")]);
			rows.Add([$"Loop[{i}].Start", loop.Start]);
			rows.Add([$"Loop[{i}].End", loop.End]);
			rows.Add([$"Loop[{i}].Fraction", loop.Fraction]);
			rows.Add([$"Loop[{i}].PlayCount", loop.PlayCount, loop.PlayCount == 0 ? "Infinite" : $"{loop.PlayCount}"]);
		}
		return rows.ToArray();
	}
}
