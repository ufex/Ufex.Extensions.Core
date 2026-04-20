using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.Heif;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.Heif;

/// <summary>
/// mdcv — Mastering Display Colour Volume node.
/// </summary>
internal class MdcvBoxNode : BoxNode
{
	public MdcvBoxNode(MdcvBox box)
		: base(box, "mdcv", "Mastering Display Colour Volume", TreeViewIcon.Properties)
	{
	}

	public override object[][] GetRows()
	{
		var box = (MdcvBox)_box;

		double maxLum = box.MaxDisplayMasteringLuminance / 10000.0;
		double minLum = box.MinDisplayMasteringLuminance / 10000.0;

		return [
			[ "Red Primary X", box.DisplayPrimariesX[0], $"{box.DisplayPrimariesX[0] / 50000.0:F4}" ],
			[ "Red Primary Y", box.DisplayPrimariesY[0], $"{box.DisplayPrimariesY[0] / 50000.0:F4}" ],
			[ "Green Primary X", box.DisplayPrimariesX[1], $"{box.DisplayPrimariesX[1] / 50000.0:F4}" ],
			[ "Green Primary Y", box.DisplayPrimariesY[1], $"{box.DisplayPrimariesY[1] / 50000.0:F4}" ],
			[ "Blue Primary X", box.DisplayPrimariesX[2], $"{box.DisplayPrimariesX[2] / 50000.0:F4}" ],
			[ "Blue Primary Y", box.DisplayPrimariesY[2], $"{box.DisplayPrimariesY[2] / 50000.0:F4}" ],
			[ "White Point X", box.WhitePointX, $"{box.WhitePointX / 50000.0:F4}" ],
			[ "White Point Y", box.WhitePointY, $"{box.WhitePointY / 50000.0:F4}" ],
			[ "Max Luminance", box.MaxDisplayMasteringLuminance, $"{maxLum:F4} nits" ],
			[ "Min Luminance", box.MinDisplayMasteringLuminance, $"{minLum:F4} nits" ],
		];
	}
}
