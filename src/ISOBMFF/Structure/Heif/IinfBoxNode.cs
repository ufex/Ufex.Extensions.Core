using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.Heif;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.Heif;

/// <summary>
/// iinf — Item Information Box node.
/// </summary>
internal class IinfBoxNode : BoxNode
{
	public IinfBoxNode(IinfBox box)
		: base(box, "iinf", "Item Information", TreeViewIcon.List)
	{
	}

	public override object[][] GetRows()
	{
		var box = (IinfBox)_box;
		return [
			[ "Entry Count", box.EntryCount, "" ],
		];
	}
}
