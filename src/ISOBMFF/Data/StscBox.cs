using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// stsc — Sample-to-Chunk Box.
/// Run-length encoded mapping of samples to chunks and sample description indices.
/// </summary>
internal class StscBox : Box
{
	public UInt32 EntryCount { get; init; }
	public StscEntry[] Entries { get; init; }

	public StscBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		EntryCount = fr.ReadUInt32();
		Entries = new StscEntry[EntryCount];
		for (int i = 0; i < EntryCount; i++)
		{
			Entries[i] = new StscEntry
			{
				FirstChunk = fr.ReadUInt32(),
				SamplesPerChunk = fr.ReadUInt32(),
				SampleDescriptionIndex = fr.ReadUInt32()
			};
		}
	}
}

internal struct StscEntry
{
	public UInt32 FirstChunk { get; init; }
	public UInt32 SamplesPerChunk { get; init; }
	public UInt32 SampleDescriptionIndex { get; init; }
}
