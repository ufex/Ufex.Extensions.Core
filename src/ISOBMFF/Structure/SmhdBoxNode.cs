using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF.Structure;

/// <summary>
/// smhd — Sound Media Header node.
/// </summary>
internal class SmhdBoxNode : BoxNode
{
	public SmhdBoxNode(SmhdBox box)
		: base(box, "smhd", "Sound Media Header", TreeViewIcon.Properties)
	{
	}

	public override object[][] GetRows()
	{
		var smhd = (SmhdBox)_box;
		double balance = smhd.Balance / 256.0;
		string balanceDesc = balance == 0 ? "Centre" : balance < 0 ? $"{balance:F2} (left)" : $"{balance:F2} (right)";

		return [
			[ "Balance", smhd.Balance, balanceDesc ],
			[ "Reserved", smhd.Reserved1, "" ],
		];
	}
}
