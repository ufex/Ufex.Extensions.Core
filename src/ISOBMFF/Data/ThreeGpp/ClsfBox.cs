using System;
using System.Text;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.ThreeGpp;

/// <summary>
/// clsf — Classification Box. Stores content classification for parental control
/// or content cataloguing systems.
/// </summary>
internal class ClsfBox : Box
{
	/// <summary>
	/// FourCC identifying the classification scheme authority.
	/// </summary>
	public Byte[] ClassificationEntity { get; init; }   // 4 bytes

	/// <summary>
	/// Table index within the classification scheme.
	/// </summary>
	public UInt16 ClassificationTable { get; init; }

	/// <summary>
	/// Packed ISO 639-2/T language code.
	/// </summary>
	public UInt16 Language { get; init; }

	/// <summary>
	/// Classification description text.
	/// </summary>
	public Byte[] ClassificationInfo { get; init; }

	public string ClassificationEntityString => Encoding.ASCII.GetString(ClassificationEntity).TrimEnd('\0');

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

	public string ClassificationInfoString
	{
		get
		{
			if (ClassificationInfo.Length == 0) return "";
			if (ClassificationInfo.Length >= 2 && ClassificationInfo[0] == 0xFE && ClassificationInfo[1] == 0xFF)
				return Encoding.BigEndianUnicode.GetString(ClassificationInfo, 2, ClassificationInfo.Length - 2).TrimEnd('\0');
			if (ClassificationInfo.Length >= 2 && ClassificationInfo[0] == 0xFF && ClassificationInfo[1] == 0xFE)
				return Encoding.Unicode.GetString(ClassificationInfo, 2, ClassificationInfo.Length - 2).TrimEnd('\0');
			return Encoding.UTF8.GetString(ClassificationInfo).TrimEnd('\0');
		}
	}

	public ClsfBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		ClassificationEntity = fr.ReadBytes(4);
		ClassificationTable = fr.ReadUInt16();
		Language = fr.ReadUInt16();

		Int64 payloadEnd = Offset + (Int64)ActualSize;
		Int64 remaining = payloadEnd - fr.BaseStream.Position;
		if (remaining > 0)
			ClassificationInfo = fr.ReadBytes((int)remaining);
		else
			ClassificationInfo = [];
	}
}
