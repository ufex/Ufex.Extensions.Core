using System;
using Ufex.API;
using Ufex.API.Format;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF.Structure;

/// <summary>
/// stsz — Sample Size node.
/// </summary>
internal class StszBoxNode : BoxNode
{
	public StszBoxNode(StszBox box)
		: base(box, "stsz", "Sample Sizes", TreeViewIcon.Table)
	{
	}

	public override object[][] GetRows()
	{
		var stsz = (StszBox)_box;

		if (stsz.SampleSize != 0)
		{
			return [
				[ "Sample Size", stsz.SampleSize, $"Constant: {ByteCountFormatter.Format(stsz.SampleSize)}" ],
				[ "Sample Count", stsz.SampleCount, $"{stsz.SampleCount} samples" ],
			];
		}

		return [
			[ "Sample Size", stsz.SampleSize, "Variable (per-sample sizes follow)" ],
			[ "Sample Count", stsz.SampleCount, $"{stsz.SampleCount} samples" ],
		];
	}
}
