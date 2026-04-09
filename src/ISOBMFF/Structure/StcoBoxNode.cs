using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF.Structure;

/// <summary>
/// stco — Chunk Offset (32-bit) node.
/// </summary>
internal class StcoBoxNode : BoxNode
{
	public StcoBoxNode(StcoBox box)
		: base(box, "stco", "Chunk Offsets (32-bit)", TreeViewIcon.Table)
	{
	}

	public override object[][] GetRows()
	{
		var stco = (StcoBox)_box;
		return [
			[ "Entry Count", stco.EntryCount, $"{stco.EntryCount} chunks" ],
		];
	}
}
