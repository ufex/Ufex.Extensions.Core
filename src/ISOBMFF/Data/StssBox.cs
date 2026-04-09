using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// stss — Sync Sample Box.
/// Lists sample numbers that are sync (random access) points (key frames).
/// If absent, every sample is a sync sample.
/// </summary>
internal class StssBox : Box
{
	public UInt32 EntryCount { get; init; }
	public UInt32[] SampleNumbers { get; init; }

	public StssBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		EntryCount = fr.ReadUInt32();
		SampleNumbers = new UInt32[EntryCount];
		for (int i = 0; i < EntryCount; i++)
		{
			SampleNumbers[i] = fr.ReadUInt32();
		}
	}
}
