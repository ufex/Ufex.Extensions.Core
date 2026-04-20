using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.Heif;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.Heif;

/// <summary>
/// ispe — Image Spatial Extents node.
/// </summary>
internal class IspeBoxNode : BoxNode
{
	public IspeBoxNode(IspeBox box)
		: base(box, "ispe", "Image Spatial Extents", TreeViewIcon.Image)
	{
	}

	public override object[][] GetRows()
	{
		var box = (IspeBox)_box;
		return [
			[ "Image Width", box.ImageWidth, $"{box.ImageWidth} pixels" ],
			[ "Image Height", box.ImageHeight, $"{box.ImageHeight} pixels" ],
		];
	}
}
