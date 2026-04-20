using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.Heif;

/// <summary>
/// ipma — Item Property Association Box. Associates items with property indices from ipco.
/// </summary>
internal class IpmaBox : Box
{
	public IpmaEntry[] Entries { get; init; }

	public IpmaBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		Int64 payloadEnd = Offset + (Int64)ActualSize;
		Byte ver = Version ?? 0;
		UInt32 flags = Flags ?? 0;

		UInt32 entryCount = fr.ReadUInt32();
		entryCount = Math.Min(entryCount, 10000);

		Entries = new IpmaEntry[entryCount];
		for (UInt32 i = 0; i < entryCount && fr.BaseStream.Position < payloadEnd; i++)
		{
			UInt32 itemId;
			if (ver < 1)
				itemId = fr.ReadUInt16();
			else
				itemId = fr.ReadUInt32();

			Byte associationCount = fr.ReadByte();
			var associations = new IpmaAssociation[associationCount];

			for (Int32 a = 0; a < associationCount && fr.BaseStream.Position < payloadEnd; a++)
			{
				bool essential;
				UInt16 propertyIndex;

				if ((flags & 0x01) != 0)
				{
					// 16-bit: 1 bit essential + 15 bits property index
					UInt16 val = fr.ReadUInt16();
					essential = (val & 0x8000) != 0;
					propertyIndex = (UInt16)(val & 0x7FFF);
				}
				else
				{
					// 8-bit: 1 bit essential + 7 bits property index
					Byte val = fr.ReadByte();
					essential = (val & 0x80) != 0;
					propertyIndex = (UInt16)(val & 0x7F);
				}

				associations[a] = new IpmaAssociation
				{
					Essential = essential,
					PropertyIndex = propertyIndex,
				};
			}

			Entries[i] = new IpmaEntry
			{
				ItemId = itemId,
				Associations = associations,
			};
		}
	}
}

internal struct IpmaEntry
{
	public UInt32 ItemId { get; init; }
	public IpmaAssociation[] Associations { get; init; }
}

internal struct IpmaAssociation
{
	public bool Essential { get; init; }
	public UInt16 PropertyIndex { get; init; }
}
