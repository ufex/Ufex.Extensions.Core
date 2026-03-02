using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data.WebP;

/// <summary>
/// ALPH - Alpha chunk. Contains encoded alpha channel data for a frame.
/// A frame containing a VP8L chunk should not contain this chunk because
/// transparency is already part of the VP8L data.
/// </summary>
internal class AlphChunk : Chunk
{
	/// <summary>
	/// Flags byte containing reserved bits, preprocessing, filtering method,
	/// and compression method.
	/// </summary>
	public Byte Flags { get; init; }

	/// <summary>
	/// The offset in the file where the alpha bitstream data begins
	/// (after the flags byte).
	/// </summary>
	public long DataOffset { get; init; }

	/// <summary>
	/// The size of the alpha bitstream data in bytes (Size - 1 for the flags byte).
	/// </summary>
	public UInt32 DataSize => Size - 1;

	/// <summary>
	/// Preprocessing: 0 = no preprocessing, 1 = level reduction.
	/// </summary>
	public Byte Preprocessing => (Byte)((Flags >> 4) & 0x03);

	/// <summary>
	/// Filtering method: 0 = none, 1 = horizontal, 2 = vertical, 3 = gradient.
	/// </summary>
	public Byte Filtering => (Byte)((Flags >> 2) & 0x03);

	/// <summary>
	/// Compression method: 0 = no compression, 1 = WebP lossless format.
	/// </summary>
	public Byte Compression => (Byte)(Flags & 0x03);

	public AlphChunk(FileReader fr) : base(fr)
	{
		Flags = fr.ReadByte();
		DataOffset = fr.BaseStream.Position;
	}
}
