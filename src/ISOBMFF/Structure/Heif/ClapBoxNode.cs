using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.Heif;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.Heif;

/// <summary>
/// clap — Clean Aperture node.
/// </summary>
internal class ClapBoxNode : BoxNode
{
	public ClapBoxNode(ClapBox box)
		: base(box, "clap", "Clean Aperture", TreeViewIcon.Gear)
	{
	}

	public override object[][] GetRows()
	{
		var box = (ClapBox)_box;
		return [
			[ "Width Numerator", box.CleanApertureWidthN, "" ],
			[ "Width Denominator", box.CleanApertureWidthD, $"= {box.CleanApertureWidth:F2} pixels" ],
			[ "Height Numerator", box.CleanApertureHeightN, "" ],
			[ "Height Denominator", box.CleanApertureHeightD, $"= {box.CleanApertureHeight:F2} pixels" ],
			[ "Horiz Offset Numerator", box.HorizOffN, "" ],
			[ "Horiz Offset Denominator", box.HorizOffD, "" ],
			[ "Vert Offset Numerator", box.VertOffN, "" ],
			[ "Vert Offset Denominator", box.VertOffD, "" ],
		];
	}
}
