using Ufex.API;
using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.Wave;

namespace Ufex.Extensions.Core.RIFF.Structure.Wave;

/// <summary>
/// fmt - Format chunk
/// </summary>
class FmtChunkNode : ChunkNode
{
	private Dictionary<UInt16, string> FormatTagNames = new Dictionary<UInt16, string>
	{
		{ 0x0001, "WAVE_FORMAT_PCM" },
		{ 0x0003, "WAVE_FORMAT_IEEE_FLOAT" },
		{ 0x0006, "WAVE_FORMAT_ALAW" },
		{ 0x0007, "WAVE_FORMAT_MULAW" },
		{ 0x0101, "IBM_FORMAT_MULAW" },
		{ 0x0102, "IBM_FORMAT_ALAW" },
		{ 0x0103, "IBM_FORMAT_ADPCM" },
		{ 0xFFFE, "WAVE_FORMAT_EXTENSIBLE" },
	};

	public FmtChunkNode(FmtChunk chunk)
		: base(chunk, chunk.ChunkIDString, "Format chunk", TreeViewIcon.Header)
	{
	}

	public override object[][] GetRows()
	{
		var d = (FmtChunk)_chunk;
		List<object[]> rows = new List<object[]>([
			["FormatTag", d.FormatTag, FormatTagNames.ContainsKey(d.FormatTag) ? FormatTagNames[d.FormatTag] : "Unknown"],
			["Channels", d.Channels],
			["SampleRate", d.SampleRate],
			["AvgBytesPerSec", d.AvgBytesPerSec],
			["BlockAlign", d.BlockAlign]
		]);

		if(d.BitsPerSample.HasValue)
		{
			rows.Add(["BitsPerSample", d.BitsPerSample.Value]);
		}
		return rows.ToArray();
	}
}
