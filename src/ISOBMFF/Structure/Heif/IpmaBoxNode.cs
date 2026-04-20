using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.Heif;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.Heif;

/// <summary>
/// ipma — Item Property Association node.
/// </summary>
internal class IpmaBoxNode : BoxNode
{
	public IpmaBoxNode(IpmaBox box)
		: base(box, "ipma", "Item Property Association", TreeViewIcon.Table)
	{
	}

	public override object[][] GetRows()
	{
		var box = (IpmaBox)_box;
		var rows = new List<object[]>();

		rows.Add([ "Entry Count", (UInt32)box.Entries.Length, "" ]);

		for (Int32 i = 0; i < box.Entries.Length; i++)
		{
			var entry = box.Entries[i];
			rows.Add([ $"Entry[{i}] Item ID", entry.ItemId, "" ]);

			for (Int32 a = 0; a < entry.Associations.Length; a++)
			{
				var assoc = entry.Associations[a];
				string essentialStr = assoc.Essential ? "Essential" : "Non-essential";
				rows.Add([ $"Entry[{i}] Property[{a}]", assoc.PropertyIndex, essentialStr ]);
			}
		}

		return rows.ToArray();
	}
}
