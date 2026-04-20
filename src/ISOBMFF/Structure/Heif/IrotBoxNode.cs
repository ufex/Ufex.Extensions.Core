using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.Heif;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.Heif;

/// <summary>
/// irot — Image Rotation node.
/// </summary>
internal class IrotBoxNode : BoxNode
{
	public IrotBoxNode(IrotBox box)
		: base(box, "irot", "Image Rotation", TreeViewIcon.Gear)
	{
	}

	public override object[][] GetRows()
	{
		var box = (IrotBox)_box;
		return [
			[ "Angle", box.Angle, $"{box.AngleDegrees}° clockwise" ],
		];
	}
}
