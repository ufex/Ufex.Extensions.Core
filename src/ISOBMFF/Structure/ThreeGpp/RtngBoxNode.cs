using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.ThreeGpp;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.ThreeGpp;

/// <summary>
/// rtng — Rating Box node. Displays rating entity, criteria, and info.
/// </summary>
internal class RtngBoxNode : BoxNode
{
	public RtngBoxNode(RtngBox box)
		: base(box, "rtng", "Rating", TreeViewIcon.Properties)
	{
	}

	public override object[][] GetRows()
	{
		var box = (RtngBox)_box;
		return [
			[ "Rating Entity", box.RatingEntity, box.RatingEntityString ],
			[ "Rating Criteria", box.RatingCriteria, box.RatingCriteriaString ],
			[ "Language", box.Language, box.LanguageString ],
			[ "Rating Info", box.RatingInfo, box.RatingInfoString ],
		];
	}
}
