using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.Heif;

/// <summary>
/// iinf — Item Information Box. A container listing one infe (Item Information Entry)
/// box per item. Has a FullBox header plus an entry_count field before the child boxes.
/// </summary>
internal class IinfBox : Box
{
	/// <summary>
	/// Number of item information entries.
	/// </summary>
	public UInt32 EntryCount { get; init; }

	public IinfBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		if (Version == 0)
			EntryCount = fr.ReadUInt16();
		else
			EntryCount = fr.ReadUInt32();

		// Read child infe boxes
		Int64 payloadEnd = Offset + (Int64)ActualSize;
		Children = ReadSubBoxes(fr, payloadEnd);
	}
}
