using System;
using System.Linq;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF.Structure;

/// <summary>
/// ftyp — File Type Box node. Displays brand and compatibility information.
/// </summary>
internal class FtypBoxNode : BoxNode
{
	public FtypBoxNode(FtypBox box)
		: base(box, "ftyp", "File Type", TreeViewIcon.Information)
	{
	}

	public override object[][] GetRows()
	{
		var ftyp = (FtypBox)_box;
		string majorDesc = BoxTypes.Brands.GetValueOrDefault(ftyp.MajorBrandString, "");

		var rows = new List<object[]>();
		rows.Add([ "Major Brand", ftyp.MajorBrand, majorDesc ]);
		rows.Add([ "Minor Version", ftyp.MinorVersion, "" ]);

		for (int i = 0; i < ftyp.CompatibleBrands.Length; i++)
		{
			string brand = System.Text.Encoding.ASCII.GetString(ftyp.CompatibleBrands[i]);
			string brandDesc = BoxTypes.Brands.GetValueOrDefault(brand, "");
			rows.Add([ $"Compatible Brand [{i}]", ftyp.CompatibleBrands[i], brandDesc ]);
		}

		return rows.ToArray();
	}
}
