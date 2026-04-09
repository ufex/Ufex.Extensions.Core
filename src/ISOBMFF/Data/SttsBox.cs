using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// stts — Decoding Time-to-Sample Box.
/// Run-length encoded table mapping sample indices to decoding timestamps.
/// </summary>
internal class SttsBox : Box
{
	public UInt32 EntryCount { get; init; }
	public SttsEntry[] Entries { get; init; }

	public SttsBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		EntryCount = fr.ReadUInt32();
		Entries = new SttsEntry[EntryCount];
		for (int i = 0; i < EntryCount; i++)
		{
			Entries[i] = new SttsEntry
			{
				SampleCount = fr.ReadUInt32(),
				SampleDelta = fr.ReadUInt32()
			};
		}
	}
}

internal struct SttsEntry
{
	public UInt32 SampleCount { get; init; }
	public UInt32 SampleDelta { get; init; }
}
