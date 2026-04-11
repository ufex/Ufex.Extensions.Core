using System;
using System.Text;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.ThreeGpp;

/// <summary>
/// rtng — Rating Box. Stores a content rating from a defined rating system.
/// </summary>
internal class RtngBox : Box
{
	/// <summary>
	/// FourCC identifying the rating organisation (e.g. 'MPAA', 'ESRB').
	/// </summary>
	public Byte[] RatingEntity { get; init; }   // 4 bytes

	/// <summary>
	/// FourCC identifying the specific rating value.
	/// </summary>
	public Byte[] RatingCriteria { get; init; }   // 4 bytes

	/// <summary>
	/// Packed ISO 639-2/T language code.
	/// </summary>
	public UInt16 Language { get; init; }

	/// <summary>
	/// Optional human-readable rating description.
	/// </summary>
	public Byte[] RatingInfo { get; init; }

	public string RatingEntityString => Encoding.ASCII.GetString(RatingEntity).TrimEnd('\0');
	public string RatingCriteriaString => Encoding.ASCII.GetString(RatingCriteria).TrimEnd('\0');

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

	public string RatingInfoString
	{
		get
		{
			if (RatingInfo.Length == 0) return "";
			if (RatingInfo.Length >= 2 && RatingInfo[0] == 0xFE && RatingInfo[1] == 0xFF)
				return Encoding.BigEndianUnicode.GetString(RatingInfo, 2, RatingInfo.Length - 2).TrimEnd('\0');
			if (RatingInfo.Length >= 2 && RatingInfo[0] == 0xFF && RatingInfo[1] == 0xFE)
				return Encoding.Unicode.GetString(RatingInfo, 2, RatingInfo.Length - 2).TrimEnd('\0');
			return Encoding.UTF8.GetString(RatingInfo).TrimEnd('\0');
		}
	}

	public RtngBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		RatingEntity = fr.ReadBytes(4);
		RatingCriteria = fr.ReadBytes(4);
		Language = fr.ReadUInt16();

		Int64 payloadEnd = Offset + (Int64)ActualSize;
		Int64 remaining = payloadEnd - fr.BaseStream.Position;
		if (remaining > 0)
			RatingInfo = fr.ReadBytes((int)remaining);
		else
			RatingInfo = [];
	}
}
