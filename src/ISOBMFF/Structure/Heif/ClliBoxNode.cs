using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.Heif;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.Heif;

/// <summary>
/// clli — Content Light Level Info node.
/// </summary>
internal class ClliBoxNode : BoxNode
{
	public ClliBoxNode(ClliBox box)
		: base(box, "clli", "Content Light Level Info", TreeViewIcon.Properties)
	{
	}

	public override object[][] GetRows()
	{
		var box = (ClliBox)_box;
		return [
			[ "Max Content Light Level", box.MaxContentLightLevel, box.MaxContentLightLevel > 0 ? $"{box.MaxContentLightLevel} nits" : "Unspecified" ],
			[ "Max Pic Average Light Level", box.MaxPicAverageLightLevel, box.MaxPicAverageLightLevel > 0 ? $"{box.MaxPicAverageLightLevel} nits" : "Unspecified" ],
		];
	}
}
