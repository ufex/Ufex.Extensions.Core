using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.ThreeGpp;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.ThreeGpp;

/// <summary>
/// Shared TreeNode for 3GPP text metadata boxes (titl, dscp, cprt, perf, auth, gnre).
/// </summary>
internal class TextMetadataBoxNode : BoxNode
{
	private static readonly Dictionary<string, (string Name, TreeViewIcon Icon)> BoxInfo = new()
	{
		{ "titl", ("Title", TreeViewIcon.Text) },
		{ "dscp", ("Description", TreeViewIcon.Text) },
		{ "cprt", ("Copyright", TreeViewIcon.Text) },
		{ "perf", ("Performer", TreeViewIcon.Text) },
		{ "auth", ("Author", TreeViewIcon.Text) },
		{ "gnre", ("Genre", TreeViewIcon.Properties) },
	};

	public TextMetadataBoxNode(TextMetadataBox box)
		: base(box, box.TypeString.Trim(), GetDescription(box), GetIcon(box))
	{
	}

	private static string GetDescription(TextMetadataBox box)
	{
		string type = box.TypeString.Trim();
		return BoxInfo.TryGetValue(type, out var info) ? info.Name : "Text Metadata";
	}

	private static TreeViewIcon GetIcon(TextMetadataBox box)
	{
		string type = box.TypeString.Trim();
		return BoxInfo.TryGetValue(type, out var info) ? info.Icon : TreeViewIcon.Text;
	}

	public override object[][] GetRows()
	{
		var box = (TextMetadataBox)_box;
		return [
			[ "Language", box.Language, box.LanguageString ],
			[ "Text", box.Text, box.TextString ],
		];
	}
}
