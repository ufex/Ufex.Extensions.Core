using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF.Structure;

/// <summary>
/// stsd — Sample Description node.
/// Shows codec entries found in the sample description box.
/// </summary>
internal class StsdBoxNode : BoxNode
{
	/// <summary>
	/// Common codec FourCC descriptions.
	/// </summary>
	private static readonly Dictionary<string, string> CodecNames = new()
	{
		{ "avc1", "H.264 / AVC" },
		{ "avc3", "H.264 / AVC (in-band params)" },
		{ "hvc1", "H.265 / HEVC" },
		{ "hev1", "H.265 / HEVC (in-band params)" },
		{ "av01", "AV1" },
		{ "vp09", "VP9" },
		{ "mp4v", "MPEG-4 Visual" },
		{ "mp4a", "MPEG-4 Audio (AAC)" },
		{ "ac-3", "AC-3 / Dolby Digital" },
		{ "ec-3", "E-AC-3 / Dolby Digital Plus" },
		{ "Opus", "Opus" },
		{ "fLaC", "FLAC" },
		{ "alac", "Apple Lossless (ALAC)" },
		{ "tx3g", "3GPP Timed Text" },
		{ "wvtt", "WebVTT" },
		{ "c608", "CEA-608 Closed Captions" },
		{ "c708", "CEA-708 Closed Captions" },
	};

	public StsdBoxNode(StsdBox box)
		: base(box, "stsd", "Sample Descriptions", TreeViewIcon.Gear)
	{
	}

	public override object[][] GetRows()
	{
		var stsd = (StsdBox)_box;
		var rows = new List<object[]>();
		rows.Add([ "Entry Count", stsd.EntryCount, "" ]);

		for (int i = 0; i < stsd.Entries.Length; i++)
		{
			var entry = stsd.Entries[i];
			if (entry.Format == null)
				continue;

			string formatStr = entry.FormatString;
			string codecDesc = CodecNames.GetValueOrDefault(formatStr, "");
			rows.Add([ $"Entry[{i}].Format", entry.Format, codecDesc ]);
			rows.Add([ $"Entry[{i}].Size", entry.Size, $"{entry.Size} bytes" ]);
		}

		return rows.ToArray();
	}
}
