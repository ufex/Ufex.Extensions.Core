using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF.Structure;

/// <summary>
/// ctts — Composition Time Offset node.
/// </summary>
internal class CttsBoxNode : BoxNode
{
	private const int MaxDisplayEntries = 50;

	public CttsBoxNode(CttsBox box)
		: base(box, "ctts", "Composition Time Offset", TreeViewIcon.Table)
	{
	}

	public override object[][] GetRows()
	{
		var ctts = (CttsBox)_box;
		var rows = new List<object[]>();
		string versionNote = ctts.Version == 1 ? "signed offsets" : "unsigned offsets";
		rows.Add([ "Entry Count", ctts.EntryCount, $"{ctts.EntryCount} entries ({versionNote})" ]);

		int displayCount = (int)Math.Min(ctts.EntryCount, MaxDisplayEntries);
		for (int i = 0; i < displayCount; i++)
		{
			var entry = ctts.Entries[i];
			string offsetDesc;
			if (ctts.Version == 1)
				offsetDesc = $"{(Int32)entry.SampleOffset} ticks";
			else
				offsetDesc = $"{entry.SampleOffset} ticks";

			rows.Add([ $"Entry[{i}].Sample Count", entry.SampleCount, $"{entry.SampleCount} samples" ]);
			rows.Add([ $"Entry[{i}].Sample Offset", entry.SampleOffset, offsetDesc ]);
		}

		if (ctts.EntryCount > MaxDisplayEntries)
		{
			rows.Add([ "...", (UInt32)0, $"{ctts.EntryCount - MaxDisplayEntries} more entries not shown" ]);
		}

		return rows.ToArray();
	}
}
