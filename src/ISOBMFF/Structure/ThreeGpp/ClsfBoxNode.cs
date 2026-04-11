using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.ThreeGpp;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.ThreeGpp;

/// <summary>
/// clsf — Classification Box node. Displays classification entity, table, and info.
/// </summary>
internal class ClsfBoxNode : BoxNode
{
	public ClsfBoxNode(ClsfBox box)
		: base(box, "clsf", "Classification", TreeViewIcon.Properties)
	{
	}

	public override object[][] GetRows()
	{
		var box = (ClsfBox)_box;
		return [
			[ "Classification Entity", box.ClassificationEntity, box.ClassificationEntityString ],
			[ "Classification Table", box.ClassificationTable, "" ],
			[ "Language", box.Language, box.LanguageString ],
			[ "Classification Info", box.ClassificationInfo, box.ClassificationInfoString ],
		];
	}
}
