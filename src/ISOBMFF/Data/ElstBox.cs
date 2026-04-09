using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// elst — Edit List Box.
/// Provides a mapping from movie time to media time for a track.
/// </summary>
internal class ElstBox : Box
{
	public UInt32 EntryCount { get; init; }
	public ElstEntry[] Entries { get; init; }

	public ElstBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		EntryCount = fr.ReadUInt32();
		Entries = new ElstEntry[EntryCount];
		for (int i = 0; i < EntryCount; i++)
		{
			if (Version == 1)
			{
				Entries[i] = new ElstEntry
				{
					SegmentDurationLong = fr.ReadUInt64(),
					MediaTimeLong = fr.ReadInt64(),
					MediaRateInteger = fr.ReadInt16(),
					MediaRateFraction = fr.ReadInt16()
				};
			}
			else
			{
				Entries[i] = new ElstEntry
				{
					SegmentDuration = fr.ReadUInt32(),
					MediaTime = fr.ReadInt32(),
					MediaRateInteger = fr.ReadInt16(),
					MediaRateFraction = fr.ReadInt16()
				};
			}
		}
	}
}

internal struct ElstEntry
{
	// Version 0 fields
	public UInt32 SegmentDuration { get; init; }
	public Int32 MediaTime { get; init; }

	// Version 1 fields
	public UInt64 SegmentDurationLong { get; init; }
	public Int64 MediaTimeLong { get; init; }

	// Shared
	public Int16 MediaRateInteger { get; init; }
	public Int16 MediaRateFraction { get; init; }
}
