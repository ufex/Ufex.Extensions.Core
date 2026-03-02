using Ufex.API;
using Ufex.API.Types;

namespace Ufex.Extensions.Core.RIFF.Data.WebP;

/// <summary>
/// ANMF - Animation frame chunk. Contains information about a single
/// animation frame including position, dimensions, duration, and the
/// frame's image data as sub-chunks (ALPH, VP8/VP8L).
/// </summary>
internal class AnmfChunk : Chunk
{
	/// <summary>
	/// Sub-chunk types that can appear in the frame data.
	/// </summary>
	private static readonly Dictionary<string, Type> FRAME_CHUNK_TYPES = new()
	{
		{ "ALPH", typeof(AlphChunk) },
		{ "VP8 ", typeof(Vp8Chunk) },
		{ "VP8L", typeof(Vp8lChunk) }
	};

	/// <summary>
	/// The X coordinate of the upper left corner of the frame is FrameX * 2.
	/// Stored as a 24-bit unsigned integer.
	/// </summary>
	public UInt24 FrameX { get; init; }

	/// <summary>
	/// The Y coordinate of the upper left corner of the frame is FrameY * 2.
	/// Stored as a 24-bit unsigned integer.
	/// </summary>
	public UInt24 FrameY { get; init; }

	/// <summary>
	/// 1-based frame width stored as a 24-bit value.
	/// The actual frame width is FrameWidthMinusOne + 1.
	/// </summary>
	public UInt24 FrameWidthMinusOne { get; init; }

	/// <summary>
	/// 1-based frame height stored as a 24-bit value.
	/// The actual frame height is FrameHeightMinusOne + 1.
	/// </summary>
	public UInt24 FrameHeightMinusOne { get; init; }

	/// <summary>
	/// The time to wait before displaying the next frame, in 1-millisecond units.
	/// Stored as a 24-bit unsigned integer.
	/// </summary>
	public UInt24 FrameDuration { get; init; }

	/// <summary>
	/// Flags byte containing reserved bits, blending method, and disposal method.
	/// </summary>
	public Byte Flags { get; init; }

	/// <summary>
	/// Actual frame width in pixels.
	/// </summary>
	public uint FrameWidth => (uint)FrameWidthMinusOne + 1;

	/// <summary>
	/// Actual frame height in pixels.
	/// </summary>
	public uint FrameHeight => (uint)FrameHeightMinusOne + 1;

	/// <summary>
	/// Blending method: 0 = alpha-blending, 1 = do not blend.
	/// </summary>
	public Byte BlendingMethod => (Byte)((Flags >> 1) & 0x01);

	/// <summary>
	/// Disposal method: 0 = do not dispose, 1 = dispose to background color.
	/// </summary>
	public Byte DisposalMethod => (Byte)(Flags & 0x01);

	/// <summary>
	/// Sub-chunks contained in the frame data (ALPH, VP8/VP8L, unknown).
	/// </summary>
	public List<Chunk> FrameChunks { get; init; } = [];

	public AnmfChunk(FileReader fr) : base(fr)
	{
		FrameX = fr.ReadUInt24();
		FrameY = fr.ReadUInt24();
		FrameWidthMinusOne = fr.ReadUInt24();
		FrameHeightMinusOne = fr.ReadUInt24();
		FrameDuration = fr.ReadUInt24();
		Flags = fr.ReadByte();

		// Read frame sub-chunks from the remaining data (Size - 16 bytes of fixed fields)
		UInt32 frameDataSize = Size - 16;
		if (frameDataSize > 0)
		{
			FrameChunks = Chunk.ReadSubChunks(fr, frameDataSize, FRAME_CHUNK_TYPES);
		}
	}
}
