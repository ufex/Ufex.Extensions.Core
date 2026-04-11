using System;
using System.Text;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.ThreeGpp;

/// <summary>
/// Base class for 3GPP text metadata boxes (titl, dscp, cprt, perf, auth, gnre).
/// All share the same structure: FullBox + language (uint16) + null-terminated UTF string.
/// Text encoding is determined by BOM: 0xFEFF = UTF-16 BE, 0xFFFE = UTF-16 LE, no BOM = UTF-8.
/// </summary>
internal class TextMetadataBox : Box
{
	/// <summary>
	/// Packed ISO 639-2/T language code (3 x 5-bit characters).
	/// </summary>
	public UInt16 Language { get; init; }

	/// <summary>
	/// The raw text bytes (may be UTF-8 or UTF-16 with BOM).
	/// </summary>
	public Byte[] Text { get; init; }

	/// <summary>
	/// Decodes the packed language code to a 3-character ISO 639-2/T string.
	/// </summary>
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

	/// <summary>
	/// Decoded text string, handling UTF-8/UTF-16 BOM detection.
	/// </summary>
	public string TextString
	{
		get
		{
			if (Text.Length == 0) return "";

			// Check for UTF-16 BOM
			if (Text.Length >= 2)
			{
				if (Text[0] == 0xFE && Text[1] == 0xFF)
					return Encoding.BigEndianUnicode.GetString(Text, 2, Text.Length - 2).TrimEnd('\0');
				if (Text[0] == 0xFF && Text[1] == 0xFE)
					return Encoding.Unicode.GetString(Text, 2, Text.Length - 2).TrimEnd('\0');
			}

			// Default: UTF-8
			return Encoding.UTF8.GetString(Text).TrimEnd('\0');
		}
	}

	public TextMetadataBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		Language = fr.ReadUInt16();

		Int64 payloadEnd = Offset + (Int64)ActualSize;
		Int64 remaining = payloadEnd - fr.BaseStream.Position;
		if (remaining > 0)
			Text = fr.ReadBytes((int)remaining);
		else
			Text = [];
	}
}
