using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF.Structure;

/// <summary>
/// stts — Decoding Time-to-Sample node.
/// </summary>
internal class SttsBoxNode : BoxNode
{
	public SttsBoxNode(SttsBox box)
		: base(box, "stts", "Decoding Time-to-Sample", TreeViewIcon.Table)
	{
	}

	public override object[][] GetRows()
	{
		var stts = (SttsBox)_box;
		var rows = new List<object[]>();
		rows.Add([ "Entry Count", stts.EntryCount, "" ]);

		for (int i = 0; i < stts.Entries.Length; i++)
		{
			var entry = stts.Entries[i];
			rows.Add([ $"Entry[{i}].Sample Count", entry.SampleCount, $"{entry.SampleCount} samples" ]);
			rows.Add([ $"Entry[{i}].Sample Delta", entry.SampleDelta, $"{entry.SampleDelta} ticks" ]);
		}

		return rows.ToArray();
	}
}
