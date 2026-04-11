using System;
using System.Text;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.ThreeGpp;

/// <summary>
/// kywd — Keyword Box. Stores a list of searchable keywords for content discovery.
/// </summary>
internal class KywdBox : Box
{
	/// <summary>
	/// Packed ISO 639-2/T language code.
	/// </summary>
	public UInt16 Language { get; init; }

	/// <summary>
	/// Number of keyword entries.
	/// </summary>
	public Byte KeywordCount { get; init; }

	/// <summary>
	/// Individual keyword entries (size + UTF string, not null-terminated).
	/// </summary>
	public KywdEntry[] Keywords { get; init; }

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

	public KywdBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		Language = fr.ReadUInt16();
		KeywordCount = fr.ReadByte();

		Int64 payloadEnd = Offset + (Int64)ActualSize;
		Keywords = new KywdEntry[KeywordCount];
		for (int i = 0; i < KeywordCount && fr.BaseStream.Position < payloadEnd; i++)
		{
			Byte keywordSize = fr.ReadByte();
			Byte[] keywordData = fr.ReadBytes(keywordSize);
			Keywords[i] = new KywdEntry
			{
				KeywordSize = keywordSize,
				KeywordData = keywordData
			};
		}
	}
}

internal struct KywdEntry
{
	public Byte KeywordSize { get; init; }
	public Byte[] KeywordData { get; init; }

	public string KeywordString
	{
		get
		{
			if (KeywordData.Length == 0) return "";
			if (KeywordData.Length >= 2 && KeywordData[0] == 0xFE && KeywordData[1] == 0xFF)
				return Encoding.BigEndianUnicode.GetString(KeywordData, 2, KeywordData.Length - 2);
			if (KeywordData.Length >= 2 && KeywordData[0] == 0xFF && KeywordData[1] == 0xFE)
				return Encoding.Unicode.GetString(KeywordData, 2, KeywordData.Length - 2);
			return Encoding.UTF8.GetString(KeywordData);
		}
	}
}
