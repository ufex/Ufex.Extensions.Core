using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF.Structure;

/// <summary>
/// tkhd — Track Header Box node. Displays per-track metadata.
/// </summary>
internal class TkhdBoxNode : BoxNode
{
	private const long MacToUnixEpochOffset = 2082844800;

	/// <summary>
	/// QTFF tkhd flag descriptions.
	/// </summary>
	private static readonly Dictionary<UInt32, string> QtffFlags = new()
	{
		{ 0x0001, "Track Enabled" },
		{ 0x0002, "Track In Movie" },
		{ 0x0004, "Track In Preview" },
		{ 0x0008, "Track In Poster" },
	};

	/// <summary>
	/// ISOBMFF tkhd flag descriptions.
	/// </summary>
	private static readonly Dictionary<UInt32, string> IsobmffFlags = new()
	{
		{ 0x0001, "track_enabled" },
		{ 0x0002, "track_in_movie" },
		{ 0x0004, "track_in_preview" },
		{ 0x0008, "track_size_is_aspect_ratio" },
	};

	public TkhdBoxNode(TkhdBox box)
		: base(box, "tkhd", "Track Header", TreeViewIcon.Header)
	{
	}

	public override object[][] GetRows()
	{
		var tkhd = (TkhdBox)_box;

		string creationDesc = FormatTimestamp(tkhd.CreationTime);
		string modificationDesc = FormatTimestamp(tkhd.ModificationTime);
		string flagsDesc = FormatFlags(tkhd.Flags ?? 0);
		double width = tkhd.Width / 65536.0;
		double height = tkhd.Height / 65536.0;
		double volume = tkhd.Volume / 256.0;

		if (tkhd.Version == 1)
		{
			return [
				[ "Creation Time", tkhd.CreationTime, creationDesc ],
				[ "Modification Time", tkhd.ModificationTime, modificationDesc ],
				[ "Track ID", tkhd.TrackID, "" ],
				[ "Reserved", tkhd.Reserved1, "" ],
				[ "Duration", tkhd.Duration, "" ],
				[ "Reserved", tkhd.Reserved2, "" ],
				[ "Layer", tkhd.Layer, "" ],
				[ "Alternate Group", tkhd.AlternateGroup, "" ],
				[ "Volume", tkhd.Volume, $"{volume:F2}" ],
				[ "Reserved", tkhd.Reserved3, "" ],
				[ "Matrix", tkhd.Matrix, FormatMatrix(tkhd.Matrix) ],
				[ "Width", tkhd.Width, $"{width:F1} px" ],
				[ "Height", tkhd.Height, $"{height:F1} px" ],
			];
		}

		return [
			[ "Creation Time", (UInt32)tkhd.CreationTime, creationDesc ],
			[ "Modification Time", (UInt32)tkhd.ModificationTime, modificationDesc ],
			[ "Track ID", tkhd.TrackID, "" ],
			[ "Reserved", tkhd.Reserved1, "" ],
			[ "Duration", (UInt32)tkhd.Duration, "" ],
			[ "Reserved", tkhd.Reserved2, "" ],
			[ "Layer", tkhd.Layer, "" ],
			[ "Alternate Group", tkhd.AlternateGroup, "" ],
			[ "Volume", tkhd.Volume, $"{volume:F2}" ],
			[ "Reserved", tkhd.Reserved3, "" ],
			[ "Matrix", tkhd.Matrix, FormatMatrix(tkhd.Matrix) ],
			[ "Width", tkhd.Width, $"{width:F1} px" ],
			[ "Height", tkhd.Height, $"{height:F1} px" ],
		];
	}

	private static string FormatTimestamp(UInt64 macTimestamp)
	{
		if (macTimestamp == 0) return "Not set";
		try
		{
			long unixTimestamp = (long)macTimestamp - MacToUnixEpochOffset;
			DateTimeOffset dt = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
			return dt.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss UTC");
		}
		catch
		{
			return $"Invalid ({macTimestamp})";
		}
	}

	private static string FormatFlags(UInt32 flags)
	{
		var parts = new List<string>();
		foreach (var (bit, name) in IsobmffFlags)
		{
			if ((flags & bit) != 0)
				parts.Add(name);
		}
		return parts.Count > 0 ? string.Join(", ", parts) : "None";
	}

	private static string FormatMatrix(Int32[] matrix)
	{
		if (matrix.Length != 9) return "";
		bool isIdentity = matrix[0] == 0x00010000 && matrix[1] == 0 && matrix[2] == 0
			&& matrix[3] == 0 && matrix[4] == 0x00010000 && matrix[5] == 0
			&& matrix[6] == 0 && matrix[7] == 0 && matrix[8] == 0x40000000;
		return isIdentity ? "Identity" : "Custom";
	}
}
