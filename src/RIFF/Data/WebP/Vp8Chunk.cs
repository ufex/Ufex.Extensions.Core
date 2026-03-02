using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data.WebP;

/// <summary>
/// VP8  - Lossy bitstream chunk. Contains VP8 encoded image data.
/// The data is not stored in memory because it can be very large.
/// Only the offset and size are recorded for reference.
/// </summary>
internal class Vp8Chunk : Chunk
{
	/// <summary>
	/// The offset in the file where the VP8 bitstream data begins
	/// (after the chunk header).
	/// </summary>
	public long DataOffset { get; init; }

	/// <summary>
	/// The size of the VP8 bitstream data in bytes.
	/// </summary>
	public UInt32 DataSize => Size;

	public Vp8Chunk(FileReader fr) : base(fr)
	{
		DataOffset = fr.BaseStream.Position;
	}
}
