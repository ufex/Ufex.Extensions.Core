using System;
using System.Text;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.ThreeGpp;

/// <summary>
/// loci — Location Box. Stores geographic location metadata (WGS-84 coordinates).
/// </summary>
internal class LociBox : Box
{
	/// <summary>
	/// Packed ISO 639-2/T language code.
	/// </summary>
	public UInt16 Language { get; init; }

	/// <summary>
	/// Location name (null-terminated UTF string).
	/// </summary>
	public Byte[] Name { get; init; }

	/// <summary>
	/// Location role: 0x00 = shooting location, 0x01 = real location, 0x02 = fictional.
	/// </summary>
	public Byte Role { get; init; }

	/// <summary>
	/// WGS-84 longitude in signed 16.16 fixed-point degrees (positive = East).
	/// </summary>
	public Int32 Longitude { get; init; }

	/// <summary>
	/// WGS-84 latitude in signed 16.16 fixed-point degrees (positive = North).
	/// </summary>
	public Int32 Latitude { get; init; }

	/// <summary>
	/// Altitude in signed 16.16 fixed-point metres above WGS-84 ellipsoid.
	/// </summary>
	public Int32 Altitude { get; init; }

	/// <summary>
	/// Astronomical body name (null-terminated UTF string, typically "Earth").
	/// </summary>
	public Byte[] AstronomicalBody { get; init; }

	/// <summary>
	/// Additional notes (null-terminated UTF string).
	/// </summary>
	public Byte[] AdditionalNotes { get; init; }

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

	public string NameString => DecodeUtfString(Name);
	public string AstronomicalBodyString => DecodeUtfString(AstronomicalBody);
	public string AdditionalNotesString => DecodeUtfString(AdditionalNotes);

	public double LongitudeDecimal => Longitude / 65536.0;
	public double LatitudeDecimal => Latitude / 65536.0;
	public double AltitudeDecimal => Altitude / 65536.0;

	public static readonly Dictionary<Byte, string> RoleNames = new()
	{
		{ 0x00, "Shooting location" },
		{ 0x01, "Real location" },
		{ 0x02, "Fictional location" },
	};

	public LociBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		Language = fr.ReadUInt16();

		// Read null-terminated name string
		Int64 payloadEnd = Offset + (Int64)ActualSize;
		Name = ReadNullTerminatedBytes(fr, payloadEnd);
		Role = fr.ReadByte();
		Longitude = fr.ReadInt32();
		Latitude = fr.ReadInt32();
		Altitude = fr.ReadInt32();
		AstronomicalBody = ReadNullTerminatedBytes(fr, payloadEnd);
		AdditionalNotes = ReadNullTerminatedBytes(fr, payloadEnd);
	}

	private static Byte[] ReadNullTerminatedBytes(FileReader fr, Int64 boundary)
	{
		var bytes = new System.Collections.Generic.List<Byte>();
		while (fr.BaseStream.Position < boundary)
		{
			Byte b = fr.ReadByte();
			if (b == 0) break;
			bytes.Add(b);
		}
		return bytes.ToArray();
	}

	private static string DecodeUtfString(Byte[] data)
	{
		if (data.Length == 0) return "";
		if (data.Length >= 2 && data[0] == 0xFE && data[1] == 0xFF)
			return Encoding.BigEndianUnicode.GetString(data, 2, data.Length - 2);
		if (data.Length >= 2 && data[0] == 0xFF && data[1] == 0xFE)
			return Encoding.Unicode.GetString(data, 2, data.Length - 2);
		return Encoding.UTF8.GetString(data);
	}
}
