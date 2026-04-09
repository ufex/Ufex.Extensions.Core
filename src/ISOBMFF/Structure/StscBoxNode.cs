using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF.Structure;

/// <summary>
/// stsc — Sample-to-Chunk node.
/// </summary>
internal class StscBoxNode : BoxNode
{
	public StscBoxNode(StscBox box)
		: base(box, "stsc", "Sample-to-Chunk", TreeViewIcon.Table)
	{
	}

	public override object[][] GetRows()
	{
		var stsc = (StscBox)_box;
		var rows = new List<object[]>();
		rows.Add([ "Entry Count", stsc.EntryCount, "" ]);

		for (int i = 0; i < stsc.Entries.Length; i++)
		{
			var entry = stsc.Entries[i];
			rows.Add([ $"Entry[{i}].First Chunk", entry.FirstChunk, "" ]);
			rows.Add([ $"Entry[{i}].Samples Per Chunk", entry.SamplesPerChunk, "" ]);
			rows.Add([ $"Entry[{i}].Sample Desc Index", entry.SampleDescriptionIndex, "" ]);
		}

		return rows.ToArray();
	}
}
