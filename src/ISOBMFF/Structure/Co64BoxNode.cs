using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF.Structure;

/// <summary>
/// co64 — Chunk Offset (64-bit) node.
/// </summary>
internal class Co64BoxNode : BoxNode
{
	public Co64BoxNode(Co64Box box)
		: base(box, "co64", "Chunk Offsets (64-bit)", TreeViewIcon.Table)
	{
	}

	public override object[][] GetRows()
	{
		var co64 = (Co64Box)_box;
		return [
			[ "Entry Count", co64.EntryCount, $"{co64.EntryCount} chunks" ],
		];
	}
}
