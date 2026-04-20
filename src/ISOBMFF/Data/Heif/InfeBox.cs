using System;
using System.Text;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.Heif;

/// <summary>
/// infe — Item Information Entry Box. Describes a single item: its ID, type, name,
/// and protection status. Version 2+ uses FourCC item_type.
/// </summary>
internal class InfeBox : Box
{
	public UInt32 ItemId { get; init; }
	public UInt16 ItemProtectionIndex { get; init; }

	/// <summary>
	/// FourCC item type (version 2+). E.g. 'hvc1', 'av01', 'Exif', 'mime', 'grid'.
	/// </summary>
	public Byte[] ItemType { get; init; }

	/// <summary>
	/// Item name (null-terminated UTF-8 string).
	/// </summary>
	public string ItemName { get; init; }

	/// <summary>
	/// Content type (null-terminated UTF-8). Present for 'mime' items.
	/// </summary>
	public string ContentType { get; init; }

	/// <summary>
	/// Content encoding (null-terminated UTF-8). Optional for 'mime' items.
	/// </summary>
	public string ContentEncoding { get; init; }

	public string ItemTypeString => ItemType.Length == 4
		? Encoding.ASCII.GetString(ItemType).TrimEnd('\0')
		: "";

	public bool IsHidden => (Flags & 0x01) != 0;

	public InfeBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		Int64 payloadEnd = Offset + (Int64)ActualSize;
		Byte ver = Version ?? 0;

		ItemType = [];
		ItemName = "";
		ContentType = "";
		ContentEncoding = "";

		if (ver <= 1)
		{
			// Version 0/1: item_ID (uint16), item_protection_index (uint16), strings
			ItemId = fr.ReadUInt16();
			ItemProtectionIndex = fr.ReadUInt16();
			ItemName = ReadNullTerminatedString(fr, payloadEnd);
			if (ver == 1 && fr.BaseStream.Position < payloadEnd)
			{
				ContentType = ReadNullTerminatedString(fr, payloadEnd);
				if (fr.BaseStream.Position < payloadEnd)
					ContentEncoding = ReadNullTerminatedString(fr, payloadEnd);
			}
		}
		else
		{
			// Version 2: item_ID (uint16), version 3: item_ID (uint32)
			if (ver == 2)
				ItemId = fr.ReadUInt16();
			else
				ItemId = fr.ReadUInt32();

			ItemProtectionIndex = fr.ReadUInt16();
			ItemType = fr.ReadBytes(4);
			ItemName = ReadNullTerminatedString(fr, payloadEnd);

			string typeStr = ItemTypeString;
			if (typeStr == "mime" && fr.BaseStream.Position < payloadEnd)
			{
				ContentType = ReadNullTerminatedString(fr, payloadEnd);
				if (fr.BaseStream.Position < payloadEnd)
					ContentEncoding = ReadNullTerminatedString(fr, payloadEnd);
			}
		}
	}

	private static string ReadNullTerminatedString(FileReader fr, Int64 boundary)
	{
		var bytes = new List<Byte>();
		while (fr.BaseStream.Position < boundary)
		{
			Byte b = fr.ReadByte();
			if (b == 0) break;
			bytes.Add(b);
		}
		return Encoding.UTF8.GetString(bytes.ToArray());
	}
}
