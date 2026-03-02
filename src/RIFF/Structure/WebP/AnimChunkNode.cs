using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.WebP;

namespace Ufex.Extensions.Core.RIFF.Structure.WebP;

/// <summary>
/// Tree node for the ANIM animation parameters chunk.
/// </summary>
class AnimChunkNode : ChunkNode
{
	public AnimChunkNode(AnimChunk chunk)
		: base(chunk, chunk.ChunkIDString, "Animation", TreeViewIcon.Header)
	{
	}

	public override object[][] GetRows()
	{
		var d = (AnimChunk)_chunk;

		// Background color is stored as [Blue, Green, Red, Alpha]
		byte b = (byte)(d.BackgroundColor & 0xFF);
		byte g = (byte)((d.BackgroundColor >> 8) & 0xFF);
		byte r = (byte)((d.BackgroundColor >> 16) & 0xFF);
		byte a = (byte)((d.BackgroundColor >> 24) & 0xFF);

		return [
			["BackgroundColor", d.BackgroundColor, $"RGBA({r}, {g}, {b}, {a})"],
			["LoopCount", d.LoopCount, d.LoopCount == 0 ? "Infinite" : $"{d.LoopCount}"]
		];
	}
}
