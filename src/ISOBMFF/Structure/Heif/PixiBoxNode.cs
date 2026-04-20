using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.Heif;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.Heif;

/// <summary>
/// pixi — Pixel Information node.
/// </summary>
internal class PixiBoxNode : BoxNode
{
	public PixiBoxNode(PixiBox box)
		: base(box, "pixi", "Pixel Information", TreeViewIcon.Palette)
	{
	}

	public override object[][] GetRows()
	{
		var box = (PixiBox)_box;
		var rows = new List<object[]>();
		rows.Add([ "Num Channels", box.NumChannels, "" ]);

		for (Int32 i = 0; i < box.BitsPerChannel.Length; i++)
		{
			rows.Add([ $"Channel[{i}] Bits", box.BitsPerChannel[i], $"{box.BitsPerChannel[i]} bpc" ]);
		}

		return rows.ToArray();
	}
}
