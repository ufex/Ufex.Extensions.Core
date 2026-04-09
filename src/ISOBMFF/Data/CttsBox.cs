using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// ctts — Composition Time Offset Box.
/// Provides the offset between decoding time (DTS) and composition/presentation
/// time (PTS). Required for video codecs using B-frames.
/// Version 0 uses unsigned offsets; version 1 uses signed offsets.
/// </summary>
internal class CttsBox : Box
{
	public UInt32 EntryCount { get; init; }
	public CttsEntry[] Entries { get; init; }

	public CttsBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		EntryCount = fr.ReadUInt32();
		Entries = new CttsEntry[EntryCount];
		for (int i = 0; i < EntryCount; i++)
		{
			Entries[i] = new CttsEntry
			{
				SampleCount = fr.ReadUInt32(),
				SampleOffset = fr.ReadUInt32() // Interpreted as signed for v1
			};
		}
	}
}

internal struct CttsEntry
{
	public UInt32 SampleCount { get; init; }

	/// <summary>
	/// Version 0: unsigned offset. Version 1: treat as signed (Int32).
	/// Stored as UInt32 for consistency; cast to Int32 when Version == 1.
	/// </summary>
	public UInt32 SampleOffset { get; init; }
}
