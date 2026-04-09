using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// co64 — Chunk Offset Box (64-bit offsets).
/// Same as stco but with 64-bit chunk offsets for large files.
/// Individual offsets are not read into memory since there can be many.
/// </summary>
internal class Co64Box : Box
{
	public UInt32 EntryCount { get; init; }

	/// <summary>
	/// File offset where the chunk_offset[] data starts.
	/// </summary>
	public Int64 DataOffset { get; init; }

	public Co64Box(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		EntryCount = fr.ReadUInt32();
		DataOffset = fr.BaseStream.Position;
		// Do not read individual offsets — can be very large
	}
}
