using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// stsz — Sample Size Box.
/// Stores the size in bytes of each sample. If SampleSize is non-zero,
/// all samples have that constant size and per-sample entries are omitted.
/// Per-sample entries are not read into memory since they can be very large.
/// </summary>
internal class StszBox : Box
{
	/// <summary>
	/// If non-zero, all samples are this constant size and EntrySizes is empty.
	/// </summary>
	public UInt32 SampleSize { get; init; }

	/// <summary>
	/// Total number of samples in the track.
	/// </summary>
	public UInt32 SampleCount { get; init; }

	/// <summary>
	/// File offset where per-sample entry_size[] data starts.
	/// Only meaningful when SampleSize == 0.
	/// </summary>
	public Int64 DataOffset { get; init; }

	public StszBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		SampleSize = fr.ReadUInt32();
		SampleCount = fr.ReadUInt32();
		DataOffset = fr.BaseStream.Position;
		// Do not read per-sample sizes — can be millions of entries
	}
}
