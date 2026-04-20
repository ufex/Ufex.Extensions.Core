using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.Heif;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.Heif;

/// <summary>
/// pasp — Pixel Aspect Ratio node.
/// </summary>
internal class PaspBoxNode : BoxNode
{
	public PaspBoxNode(PaspBox box)
		: base(box, "pasp", "Pixel Aspect Ratio", TreeViewIcon.Gear)
	{
	}

	public override object[][] GetRows()
	{
		var box = (PaspBox)_box;
		string desc = box.HSpacing == box.VSpacing ? "Square pixels" : $"{box.HSpacing}:{box.VSpacing}";
		return [
			[ "H Spacing", box.HSpacing, desc ],
			[ "V Spacing", box.VSpacing, "" ],
		];
	}
}
