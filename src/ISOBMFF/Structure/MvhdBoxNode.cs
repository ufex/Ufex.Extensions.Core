using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF.Structure;

/// <summary>
/// mvhd — Movie Header Box node. Displays movie-level timing and metadata.
/// </summary>
internal class MvhdBoxNode : BoxNode
{
	/// <summary>
	/// Seconds between 1904-01-01 (Mac epoch) and 1970-01-01 (Unix epoch).
	/// </summary>
	private const long MacToUnixEpochOffset = 2082844800;

	public MvhdBoxNode(MvhdBox box)
		: base(box, "mvhd", "Movie Header", TreeViewIcon.Header)
	{
	}

	public override object[][] GetRows()
	{
		var mvhd = (MvhdBox)_box;

		string creationDesc = FormatTimestamp(mvhd.CreationTime);
		string modificationDesc = FormatTimestamp(mvhd.ModificationTime);
		double durationSec = mvhd.Timescale > 0 ? (double)mvhd.Duration / mvhd.Timescale : 0;
		string durationDesc = $"{durationSec:F3} seconds";
		double rate = mvhd.Rate / 65536.0;
		double volume = mvhd.Volume / 256.0;

		if (mvhd.Version == 1)
		{
			return [
				[ "Creation Time", mvhd.CreationTime, creationDesc ],
				[ "Modification Time", mvhd.ModificationTime, modificationDesc ],
				[ "Timescale", mvhd.Timescale, $"{mvhd.Timescale} ticks/sec" ],
				[ "Duration", mvhd.Duration, durationDesc ],
				[ "Rate", mvhd.Rate, $"{rate:F4}" ],
				[ "Volume", mvhd.Volume, $"{volume:F2}" ],
				[ "Reserved", mvhd.Reserved, "" ],
				[ "Matrix", mvhd.Matrix, FormatMatrix(mvhd.Matrix) ],
				[ "Pre-defined", mvhd.PreDefined, "" ],
				[ "Next Track ID", mvhd.NextTrackID, "" ],
			];
		}

		return [
			[ "Creation Time", (UInt32)mvhd.CreationTime, creationDesc ],
			[ "Modification Time", (UInt32)mvhd.ModificationTime, modificationDesc ],
			[ "Timescale", mvhd.Timescale, $"{mvhd.Timescale} ticks/sec" ],
			[ "Duration", (UInt32)mvhd.Duration, durationDesc ],
			[ "Rate", mvhd.Rate, $"{rate:F4}" ],
			[ "Volume", mvhd.Volume, $"{volume:F2}" ],
			[ "Reserved", mvhd.Reserved, "" ],
			[ "Matrix", mvhd.Matrix, FormatMatrix(mvhd.Matrix) ],
			[ "Pre-defined", mvhd.PreDefined, "" ],
			[ "Next Track ID", mvhd.NextTrackID, "" ],
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

	private static string FormatMatrix(Int32[] matrix)
	{
		if (matrix.Length != 9) return "";
		// Check for identity matrix
		bool isIdentity = matrix[0] == 0x00010000 && matrix[1] == 0 && matrix[2] == 0
			&& matrix[3] == 0 && matrix[4] == 0x00010000 && matrix[5] == 0
			&& matrix[6] == 0 && matrix[7] == 0 && matrix[8] == 0x40000000;
		return isIdentity ? "Identity" : "Custom";
	}
}
