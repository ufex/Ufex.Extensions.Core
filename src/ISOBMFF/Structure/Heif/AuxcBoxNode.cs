using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.Heif;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.Heif;

/// <summary>
/// auxC — Auxiliary Type Property node.
/// </summary>
internal class AuxcBoxNode : BoxNode
{
	public AuxcBoxNode(AuxcBox box)
		: base(box, "auxC", "Auxiliary Type", TreeViewIcon.Properties)
	{
	}

	public override object[][] GetRows()
	{
		var box = (AuxcBox)_box;
		var rows = new List<object[]>();

		rows.Add([ "Aux Type", box.AuxType, box.AuxTypeDescription ]);

		if (box.AuxSubtype.Length > 0)
			rows.Add([ "Aux Subtype", box.AuxSubtype, $"{box.AuxSubtype.Length} bytes" ]);

		return rows.ToArray();
	}
}
