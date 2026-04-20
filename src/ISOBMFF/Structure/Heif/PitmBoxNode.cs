using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.Heif;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.Heif;

/// <summary>
/// pitm — Primary Item Box node.
/// </summary>
internal class PitmBoxNode : BoxNode
{
	public PitmBoxNode(PitmBox box)
		: base(box, "pitm", "Primary Item", TreeViewIcon.Image)
	{
	}

	public override object[][] GetRows()
	{
		var box = (PitmBox)_box;
		return [
			[ "Item ID", box.ItemId, "" ],
		];
	}
}
