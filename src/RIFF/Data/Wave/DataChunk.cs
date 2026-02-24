using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data.Wave;

/// <summary>
/// data - Data chunk. Contains the actual audio sample data.
/// The data is not stored in memory because it can be very large.
/// Only the offset and size are recorded for reference.
/// </summary>
internal class DataChunk : Chunk
{
	/// <summary>
	/// The offset in the file where the audio data begins (after the chunk header).
	/// </summary>
	public long DataOffset { get; init; }

	/// <summary>
	/// The size of the audio data in bytes (same as Size from the base Chunk class).
	/// </summary>
	public UInt32 DataSize => Size;

	public DataChunk(FileReader fr) : base(fr)
	{
		DataOffset = fr.BaseStream.Position;
		// Do not read the data into memory - just record the offset.
		// The stream position will be advanced by ReadSubChunks after construction.
	}
}
