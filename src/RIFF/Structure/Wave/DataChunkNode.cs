using Ufex.API.Format;
using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.Wave;

namespace Ufex.Extensions.Core.RIFF.Structure.Wave;

/// <summary>
/// Tree node for the data chunk.
/// </summary>
class DataChunkNode : ChunkNode
{
	public DataChunkNode(DataChunk chunk)
		: base(chunk, chunk.ChunkIDString, "Audio data", TreeViewIcon.Binary)
	{
	}

	public override object[][] GetRows()
	{
		var d = (DataChunk)_chunk;
		return [
			["DataOffset", (UInt32)d.DataOffset, $"Byte offset of audio data"],
			["DataSize", d.DataSize, ByteCountFormatter.Format(d.DataSize)]
		];
	}
}
