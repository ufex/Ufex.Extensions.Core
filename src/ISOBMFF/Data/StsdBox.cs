using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data;

/// <summary>
/// stsd — Sample Description Box.
/// Contains one or more sample entries describing the codec configuration.
/// Each entry has a FourCC type specific to the codec (e.g. avc1, hvc1, mp4a).
/// Only the entry count and FourCC types are read for now; codec-specific
/// configuration boxes will be parsed in format-specific subfolders.
/// </summary>
internal class StsdBox : Box
{
	public UInt32 EntryCount { get; init; }
	public StsdEntry[] Entries { get; init; }

	public StsdBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		EntryCount = fr.ReadUInt32();
		Int64 payloadEnd = Offset + (Int64)ActualSize;
		Entries = new StsdEntry[EntryCount];

		for (int i = 0; i < EntryCount && fr.BaseStream.Position + 8 <= payloadEnd; i++)
		{
			Int64 entryStart = fr.BaseStream.Position;
			UInt32 entrySize = fr.ReadUInt32();
			Byte[] entryType = fr.ReadBytes(4);

			Entries[i] = new StsdEntry
			{
				Offset = entryStart,
				Size = entrySize,
				Format = entryType
			};

			// Skip to end of this entry (we don't parse codec-specific data yet)
			Int64 entryEnd = entryStart + entrySize;
			if (entryEnd > payloadEnd)
				entryEnd = payloadEnd;
			fr.BaseStream.Seek(entryEnd, SeekOrigin.Begin);
		}
	}
}

internal struct StsdEntry
{
	public Int64 Offset { get; init; }
	public UInt32 Size { get; init; }
	public Byte[] Format { get; init; }

	public string FormatString => System.Text.Encoding.ASCII.GetString(Format);
}
