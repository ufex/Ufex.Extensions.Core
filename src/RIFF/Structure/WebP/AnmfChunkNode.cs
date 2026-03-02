using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data;
using Ufex.Extensions.Core.RIFF.Data.WebP;

namespace Ufex.Extensions.Core.RIFF.Structure.WebP;

/// <summary>
/// Tree node for the ANMF animation frame chunk.
/// </summary>
class AnmfChunkNode : ChunkNode
{
	private static readonly Dictionary<byte, string> BlendingMethodNames = new()
	{
		{ 0, "Alpha-blending" },
		{ 1, "Do not blend" }
	};

	private static readonly Dictionary<byte, string> DisposalMethodNames = new()
	{
		{ 0, "Do not dispose" },
		{ 1, "Dispose to background" }
	};

	public AnmfChunkNode(AnmfChunk chunk)
		: base(chunk, chunk.ChunkIDString, "Animation frame", TreeViewIcon.Header)
	{
		// Add child nodes for frame sub-chunks (ALPH, VP8/VP8L)
		foreach (Chunk subChunk in chunk.FrameChunks)
		{
			Nodes.Add(FromChunk(subChunk));
		}
	}

	public override object[][] GetRows()
	{
		var d = (AnmfChunk)_chunk;
		return [
			["FrameX", d.FrameX, $"{(uint)d.FrameX * 2} px"],
			["FrameY", d.FrameY, $"{(uint)d.FrameY * 2} px"],
			["FrameWidthMinusOne", d.FrameWidthMinusOne, $"{d.FrameWidth} px"],
			["FrameHeightMinusOne", d.FrameHeightMinusOne, $"{d.FrameHeight} px"],
			["FrameDuration", d.FrameDuration, $"{(uint)d.FrameDuration} ms"],
			["Flags", d.Flags, $"Blend: {BlendingMethodNames.GetValueOrDefault(d.BlendingMethod, "Unknown")}, Dispose: {DisposalMethodNames.GetValueOrDefault(d.DisposalMethod, "Unknown")}"]
		];
	}
}
