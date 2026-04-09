using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF.Structure;

/// <summary>
/// elst — Edit List node.
/// </summary>
internal class ElstBoxNode : BoxNode
{
	public ElstBoxNode(ElstBox box)
		: base(box, "elst", "Edit List", TreeViewIcon.Table)
	{
	}

	public override object[][] GetRows()
	{
		var elst = (ElstBox)_box;
		var rows = new List<object[]>();
		rows.Add([ "Entry Count", elst.EntryCount, "" ]);

		for (int i = 0; i < elst.Entries.Length; i++)
		{
			var entry = elst.Entries[i];
			double rate = entry.MediaRateInteger + entry.MediaRateFraction / 65536.0;

			if (elst.Version == 1)
			{
				rows.Add([ $"Entry[{i}].Segment Duration", entry.SegmentDurationLong, "" ]);
				rows.Add([ $"Entry[{i}].Media Time", entry.MediaTimeLong, entry.MediaTimeLong == -1 ? "Empty edit" : "" ]);
			}
			else
			{
				rows.Add([ $"Entry[{i}].Segment Duration", entry.SegmentDuration, "" ]);
				rows.Add([ $"Entry[{i}].Media Time", entry.MediaTime, entry.MediaTime == -1 ? "Empty edit" : "" ]);
			}
			rows.Add([ $"Entry[{i}].Media Rate Integer", entry.MediaRateInteger, $"{rate:F4}" ]);
			rows.Add([ $"Entry[{i}].Media Rate Fraction", entry.MediaRateFraction, "" ]);
		}

		return rows.ToArray();
	}
}
