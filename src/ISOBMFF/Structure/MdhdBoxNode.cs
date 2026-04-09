using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF.Structure;

/// <summary>
/// mdhd — Media Header Box node. Displays per-track media timing metadata.
/// </summary>
internal class MdhdBoxNode : BoxNode
{
	private const long MacToUnixEpochOffset = 2082844800;

	public MdhdBoxNode(MdhdBox box)
		: base(box, "mdhd", "Media Header", TreeViewIcon.Header)
	{
	}

	public override object[][] GetRows()
	{
		var mdhd = (MdhdBox)_box;

		string creationDesc = FormatTimestamp(mdhd.CreationTime);
		string modificationDesc = FormatTimestamp(mdhd.ModificationTime);
		double durationSec = mdhd.Timescale > 0 ? (double)mdhd.Duration / mdhd.Timescale : 0;
		string durationDesc = $"{durationSec:F3} seconds";

		if (mdhd.Version == 1)
		{
			return [
				[ "Creation Time", mdhd.CreationTime, creationDesc ],
				[ "Modification Time", mdhd.ModificationTime, modificationDesc ],
				[ "Timescale", mdhd.Timescale, $"{mdhd.Timescale} ticks/sec" ],
				[ "Duration", mdhd.Duration, durationDesc ],
				[ "Language", mdhd.Language, mdhd.LanguageString ],
				[ "Pre-defined", mdhd.PreDefined, mdhd.PreDefined != 0 ? "QTFF quality indicator" : "" ],
			];
		}

		return [
			[ "Creation Time", (UInt32)mdhd.CreationTime, creationDesc ],
			[ "Modification Time", (UInt32)mdhd.ModificationTime, modificationDesc ],
			[ "Timescale", mdhd.Timescale, $"{mdhd.Timescale} ticks/sec" ],
			[ "Duration", (UInt32)mdhd.Duration, durationDesc ],
			[ "Language", mdhd.Language, mdhd.LanguageString ],
			[ "Pre-defined", mdhd.PreDefined, mdhd.PreDefined != 0 ? "QTFF quality indicator" : "" ],
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
}
