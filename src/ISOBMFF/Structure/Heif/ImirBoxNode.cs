using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.Heif;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.Heif;

/// <summary>
/// imir — Image Mirror node.
/// </summary>
internal class ImirBoxNode : BoxNode
{
	public ImirBoxNode(ImirBox box)
		: base(box, "imir", "Image Mirror", TreeViewIcon.Gear)
	{
	}

	public override object[][] GetRows()
	{
		var box = (ImirBox)_box;
		return [
			[ "Axis", box.Axis, box.AxisDescription ],
		];
	}
}
