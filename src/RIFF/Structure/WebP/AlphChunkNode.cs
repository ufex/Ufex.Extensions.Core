using Ufex.API.Format;
using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.WebP;

namespace Ufex.Extensions.Core.RIFF.Structure.WebP;

/// <summary>
/// Tree node for the ALPH alpha channel chunk.
/// </summary>
class AlphChunkNode : ChunkNode
{
	private static readonly Dictionary<byte, string> PreprocessingNames = new()
	{
		{ 0, "No preprocessing" },
		{ 1, "Level reduction" }
	};

	private static readonly Dictionary<byte, string> FilteringNames = new()
	{
		{ 0, "None" },
		{ 1, "Horizontal" },
		{ 2, "Vertical" },
		{ 3, "Gradient" }
	};

	private static readonly Dictionary<byte, string> CompressionNames = new()
	{
		{ 0, "No compression" },
		{ 1, "WebP lossless" }
	};

	public AlphChunkNode(AlphChunk chunk)
		: base(chunk, chunk.ChunkIDString, "Alpha", TreeViewIcon.Header)
	{
	}

	public override object[][] GetRows()
	{
		var d = (AlphChunk)_chunk;
		return [
			["Flags", d.Flags, $"P={d.Preprocessing}, F={d.Filtering}, C={d.Compression}"],
			["Preprocessing", d.Preprocessing, PreprocessingNames.GetValueOrDefault(d.Preprocessing, "Unknown")],
			["Filtering", d.Filtering, FilteringNames.GetValueOrDefault(d.Filtering, "Unknown")],
			["Compression", d.Compression, CompressionNames.GetValueOrDefault(d.Compression, "Unknown")],
			["DataOffset", (UInt32)d.DataOffset, "Byte offset of alpha bitstream"],
			["DataSize", d.DataSize, ByteCountFormatter.Format(d.DataSize)]
		];
	}
}
