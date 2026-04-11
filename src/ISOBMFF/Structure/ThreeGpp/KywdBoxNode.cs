using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.ThreeGpp;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.ThreeGpp;

/// <summary>
/// kywd — Keyword Box node. Displays keywords list.
/// </summary>
internal class KywdBoxNode : BoxNode
{
	public KywdBoxNode(KywdBox box)
		: base(box, "kywd", "Keywords", TreeViewIcon.List)
	{
	}

	public override object[][] GetRows()
	{
		var box = (KywdBox)_box;

		var rows = new System.Collections.Generic.List<object[]>();
		rows.Add([ "Language", box.Language, box.LanguageString ]);
		rows.Add([ "Keyword Count", box.KeywordCount, "" ]);

		for (int i = 0; i < box.Keywords.Length; i++)
		{
			rows.Add([ $"Keyword[{i}] Size", box.Keywords[i].KeywordSize, "" ]);
			rows.Add([ $"Keyword[{i}]", box.Keywords[i].KeywordData, box.Keywords[i].KeywordString ]);
		}

		return rows.ToArray();
	}
}
