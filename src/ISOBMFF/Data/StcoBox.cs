using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// stco — Chunk Offset Box (32-bit offsets).
/// Maps each chunk to its absolute byte offset within the file.
/// Individual offsets are not read into memory since there can be many.
/// </summary>
internal class StcoBox : Box
{
	public UInt32 EntryCount { get; init; }

	/// <summary>
	/// File offset where the chunk_offset[] data starts.
	/// </summary>
	public Int64 DataOffset { get; init; }

	public StcoBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		EntryCount = fr.ReadUInt32();
		DataOffset = fr.BaseStream.Position;
		// Do not read individual offsets — can be very large
	}
}
