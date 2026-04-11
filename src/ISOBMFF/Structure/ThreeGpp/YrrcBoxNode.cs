using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.ThreeGpp;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.ThreeGpp;

/// <summary>
/// yrrc — Recording Year Box node. Displays the recording year.
/// </summary>
internal class YrrcBoxNode : BoxNode
{
	public YrrcBoxNode(YrrcBox box)
		: base(box, "yrrc", "Recording Year", TreeViewIcon.Properties)
	{
	}

	public override object[][] GetRows()
	{
		var box = (YrrcBox)_box;
		return [
			[ "Recording Year", box.RecordingYear, "" ],
		];
	}
}
