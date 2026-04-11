using System;
using System.Text;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.ThreeGpp;

/// <summary>
/// albm — Album Box. Stores album title and optional track number.
/// </summary>
internal class AlbmBox : Box
{
	/// <summary>
	/// Packed ISO 639-2/T language code.
	/// </summary>
	public UInt16 Language { get; init; }

	/// <summary>
	/// Album title (null-terminated UTF string).
	/// </summary>
	public Byte[] AlbumTitle { get; init; }

	/// <summary>
	/// Optional track number. Only present if there are bytes remaining after the null-terminated title.
	/// </summary>
	public Byte? TrackNumber { get; init; }

	public string LanguageString
	{
		get
		{
			char c1 = (char)(((Language >> 10) & 0x1F) + 0x60);
			char c2 = (char)(((Language >> 5) & 0x1F) + 0x60);
			char c3 = (char)((Language & 0x1F) + 0x60);
			return new string([c1, c2, c3]);
		}
	}

	public string AlbumTitleString
	{
		get
		{
			if (AlbumTitle.Length == 0) return "";
			if (AlbumTitle.Length >= 2 && AlbumTitle[0] == 0xFE && AlbumTitle[1] == 0xFF)
				return Encoding.BigEndianUnicode.GetString(AlbumTitle, 2, AlbumTitle.Length - 2);
			if (AlbumTitle.Length >= 2 && AlbumTitle[0] == 0xFF && AlbumTitle[1] == 0xFE)
				return Encoding.Unicode.GetString(AlbumTitle, 2, AlbumTitle.Length - 2);
			return Encoding.UTF8.GetString(AlbumTitle);
		}
	}

	public AlbmBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		Language = fr.ReadUInt16();

		Int64 payloadEnd = Offset + (Int64)ActualSize;

		// Read null-terminated album title
		var bytes = new System.Collections.Generic.List<Byte>();
		while (fr.BaseStream.Position < payloadEnd)
		{
			Byte b = fr.ReadByte();
			if (b == 0) break;
			bytes.Add(b);
		}
		AlbumTitle = bytes.ToArray();

		// Optional track number if bytes remain
		if (fr.BaseStream.Position < payloadEnd)
			TrackNumber = fr.ReadByte();
	}
}
