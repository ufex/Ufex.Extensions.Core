using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.ThreeGpp;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.ThreeGpp;

/// <summary>
/// albm — Album Box node. Displays album title and optional track number.
/// </summary>
internal class AlbmBoxNode : BoxNode
{
	public AlbmBoxNode(AlbmBox box)
		: base(box, "albm", "Album", TreeViewIcon.Text)
	{
	}

	public override object[][] GetRows()
	{
		var box = (AlbmBox)_box;
		var rows = new System.Collections.Generic.List<object[]>();
		rows.Add([ "Language", box.Language, box.LanguageString ]);
		rows.Add([ "Album Title", box.AlbumTitle, box.AlbumTitleString ]);

		if (box.TrackNumber.HasValue)
			rows.Add([ "Track Number", box.TrackNumber.Value, "" ]);

		return rows.ToArray();
	}
}
