using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.Heif;

/// <summary>
/// iloc — Item Location Box. Provides byte offsets and lengths for every item in a HEIF file.
/// This is the HEIF equivalent of stco + stsz in MP4.
/// </summary>
internal class IlocBox : Box
{
	public Byte OffsetSize { get; init; }
	public Byte LengthSize { get; init; }
	public Byte BaseOffsetSize { get; init; }
	public Byte IndexSize { get; init; }
	public IlocItem[] Items { get; init; }

	public IlocBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		Int64 payloadEnd = Offset + (Int64)ActualSize;
		Byte ver = Version ?? 0;

		// First byte: offset_size (4 bits) | length_size (4 bits)
		Byte byte1 = fr.ReadByte();
		OffsetSize = (Byte)((byte1 >> 4) & 0x0F);
		LengthSize = (Byte)(byte1 & 0x0F);

		// Second byte: base_offset_size (4 bits) | index_size (4 bits, version 1+ only)
		Byte byte2 = fr.ReadByte();
		BaseOffsetSize = (Byte)((byte2 >> 4) & 0x0F);
		IndexSize = ver >= 1 ? (Byte)(byte2 & 0x0F) : (Byte)0;

		UInt32 itemCount;
		if (ver < 2)
			itemCount = fr.ReadUInt16();
		else
			itemCount = fr.ReadUInt32();

		// Cap to avoid excessive allocation on malformed files
		itemCount = Math.Min(itemCount, 10000);

		Items = new IlocItem[itemCount];
		for (UInt32 i = 0; i < itemCount && fr.BaseStream.Position < payloadEnd; i++)
		{
			UInt32 itemId;
			if (ver < 2)
				itemId = fr.ReadUInt16();
			else
				itemId = fr.ReadUInt32();

			UInt16 constructionMethod = 0;
			if (ver >= 1)
			{
				UInt16 cm = fr.ReadUInt16();
				constructionMethod = (UInt16)(cm & 0x0F);
			}

			UInt16 dataRefIndex = fr.ReadUInt16();
			UInt64 baseOffset = ReadVarUInt(fr, BaseOffsetSize);
			UInt16 extentCount = fr.ReadUInt16();

			var extents = new IlocExtent[extentCount];
			for (Int32 e = 0; e < extentCount && fr.BaseStream.Position < payloadEnd; e++)
			{
				UInt64 extentIndex = 0;
				if (ver >= 1 && IndexSize > 0)
					extentIndex = ReadVarUInt(fr, IndexSize);

				UInt64 extentOffset = ReadVarUInt(fr, OffsetSize);
				UInt64 extentLength = ReadVarUInt(fr, LengthSize);

				extents[e] = new IlocExtent
				{
					ExtentIndex = extentIndex,
					ExtentOffset = extentOffset,
					ExtentLength = extentLength,
				};
			}

			Items[i] = new IlocItem
			{
				ItemId = itemId,
				ConstructionMethod = constructionMethod,
				DataReferenceIndex = dataRefIndex,
				BaseOffset = baseOffset,
				Extents = extents,
			};
		}
	}

	private static UInt64 ReadVarUInt(FileReader fr, Byte size)
	{
		return size switch
		{
			0 => 0,
			4 => fr.ReadUInt32(),
			8 => fr.ReadUInt64(),
			_ => 0,
		};
	}
}

internal struct IlocItem
{
	public UInt32 ItemId { get; init; }
	public UInt16 ConstructionMethod { get; init; }
	public UInt16 DataReferenceIndex { get; init; }
	public UInt64 BaseOffset { get; init; }
	public IlocExtent[] Extents { get; init; }

	/// <summary>
	/// Total byte length of all extents for this item.
	/// </summary>
	public UInt64 TotalLength
	{
		get
		{
			UInt64 total = 0;
			foreach (var e in Extents)
				total += e.ExtentLength;
			return total;
		}
	}
}

internal struct IlocExtent
{
	public UInt64 ExtentIndex { get; init; }
	public UInt64 ExtentOffset { get; init; }
	public UInt64 ExtentLength { get; init; }
}
