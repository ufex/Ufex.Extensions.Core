using System;
using System.Text;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.Heif;

/// <summary>
/// auxC — Auxiliary Type Property. Declares the type of an auxiliary image (alpha, depth, etc.).
/// </summary>
internal class AuxcBox : Box
{
	/// <summary>
	/// Auxiliary type URI (null-terminated UTF-8 string).
	/// </summary>
	public string AuxType { get; init; }

	/// <summary>
	/// Auxiliary subtype data (remaining bytes after the URI).
	/// </summary>
	public Byte[] AuxSubtype { get; init; }

	public static readonly Dictionary<string, string> AuxTypeNames = new()
	{
		{ "urn:mpeg:mpegB:cicp:systems:auxiliary:alpha", "Alpha plane" },
		{ "urn:mpeg:hevc:2015:auxid:1", "HEVC alpha plane (legacy)" },
		{ "urn:mpeg:mpegB:cicp:systems:auxiliary:depth", "Depth map" },
		{ "urn:apple:photo:2020:aux:hdrgainmap", "Apple HDR gain map" },
		{ "urn:com:apple:photo:2020:aux:hdrgainmap", "Apple HDR gain map" },
	};

	public string AuxTypeDescription => AuxTypeNames.GetValueOrDefault(AuxType, "");

	public AuxcBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		Int64 payloadEnd = Offset + (Int64)ActualSize;

		// Read null-terminated UTF-8 aux_type URI
		var bytes = new List<Byte>();
		while (fr.BaseStream.Position < payloadEnd)
		{
			Byte b = fr.ReadByte();
			if (b == 0) break;
			bytes.Add(b);
		}
		AuxType = Encoding.UTF8.GetString(bytes.ToArray());

		// Remaining bytes are aux_subtype
		Int64 remaining = payloadEnd - fr.BaseStream.Position;
		if (remaining > 0)
			AuxSubtype = fr.ReadBytes((Int32)remaining);
		else
			AuxSubtype = [];
	}
}
