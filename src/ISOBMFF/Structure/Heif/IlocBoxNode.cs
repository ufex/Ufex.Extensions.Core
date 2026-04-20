using System;
using Ufex.API.Format;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.Heif;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.Heif;

/// <summary>
/// iloc — Item Location Box node. Displays item location entries.
/// </summary>
internal class IlocBoxNode : BoxNode
{
	public IlocBoxNode(IlocBox box)
		: base(box, "iloc", "Item Location", TreeViewIcon.Table)
	{
	}

	public override object[][] GetRows()
	{
		var box = (IlocBox)_box;

		var rows = new List<object[]>();
		rows.Add([ "Offset Size", box.OffsetSize, $"{box.OffsetSize} bytes" ]);
		rows.Add([ "Length Size", box.LengthSize, $"{box.LengthSize} bytes" ]);
		rows.Add([ "Base Offset Size", box.BaseOffsetSize, $"{box.BaseOffsetSize} bytes" ]);

		if ((box.Version ?? 0) >= 1)
			rows.Add([ "Index Size", box.IndexSize, $"{box.IndexSize} bytes" ]);

		rows.Add([ "Item Count", (UInt32)box.Items.Length, "" ]);

		for (Int32 i = 0; i < box.Items.Length; i++)
		{
			var item = box.Items[i];
			string methodDesc = item.ConstructionMethod switch
			{
				0 => "File offset",
				1 => "idat offset",
				2 => "Item offset",
				_ => $"Unknown ({item.ConstructionMethod})",
			};

			rows.Add([ $"Item[{i}] ID", item.ItemId, "" ]);

			if ((box.Version ?? 0) >= 1)
				rows.Add([ $"Item[{i}] Construction Method", item.ConstructionMethod, methodDesc ]);

			rows.Add([ $"Item[{i}] Data Ref Index", item.DataReferenceIndex, "" ]);

			if (box.BaseOffsetSize > 0)
				rows.Add([ $"Item[{i}] Base Offset", item.BaseOffset, $"0x{item.BaseOffset:X}" ]);

			rows.Add([ $"Item[{i}] Extent Count", (UInt16)item.Extents.Length, "" ]);

			for (Int32 e = 0; e < item.Extents.Length; e++)
			{
				var ext = item.Extents[e];
				rows.Add([ $"Item[{i}] Extent[{e}] Offset", ext.ExtentOffset, $"0x{ext.ExtentOffset:X}" ]);
				rows.Add([ $"Item[{i}] Extent[{e}] Length", ext.ExtentLength, ByteCountFormatter.Format(ext.ExtentLength) ]);
			}
		}

		return rows.ToArray();
	}
}
