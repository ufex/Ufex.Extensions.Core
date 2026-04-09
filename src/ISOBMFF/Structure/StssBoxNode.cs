using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF.Structure;

/// <summary>
/// stss — Sync Sample Table node.
/// </summary>
internal class StssBoxNode : BoxNode
{
	private const int MaxDisplayEntries = 50;

	public StssBoxNode(StssBox box)
		: base(box, "stss", "Sync Sample Table", TreeViewIcon.Table)
	{
	}

	public override object[][] GetRows()
	{
		var stss = (StssBox)_box;
		var rows = new List<object[]>();
		rows.Add([ "Entry Count", stss.EntryCount, $"{stss.EntryCount} sync samples (key frames)" ]);

		int displayCount = (int)Math.Min(stss.EntryCount, MaxDisplayEntries);
		for (int i = 0; i < displayCount; i++)
		{
			rows.Add([ $"Sync Sample [{i}]", stss.SampleNumbers[i], $"Sample #{stss.SampleNumbers[i]}" ]);
		}

		if (stss.EntryCount > MaxDisplayEntries)
		{
			rows.Add([ "...", (UInt32)0, $"{stss.EntryCount - MaxDisplayEntries} more entries not shown" ]);
		}

		return rows.ToArray();
	}
}
