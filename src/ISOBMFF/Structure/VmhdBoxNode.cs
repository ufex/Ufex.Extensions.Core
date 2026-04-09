using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF.Structure;

/// <summary>
/// vmhd — Video Media Header node.
/// </summary>
internal class VmhdBoxNode : BoxNode
{
	private static readonly Dictionary<UInt16, string> GraphicsModes = new()
	{
		{ 0x0000, "Copy" },
		{ 0x0024, "Transparent" },
		{ 0x0040, "Dither copy" },
		{ 0x0100, "Straight alpha" },
		{ 0x0101, "Premul white alpha" },
		{ 0x0102, "Premul black alpha" },
		{ 0x0104, "Composition (dither copy)" },
	};

	public VmhdBoxNode(VmhdBox box)
		: base(box, "vmhd", "Video Media Header", TreeViewIcon.Properties)
	{
	}

	public override object[][] GetRows()
	{
		var vmhd = (VmhdBox)_box;
		string modeDesc = GraphicsModes.GetValueOrDefault(vmhd.GraphicsMode, $"Unknown (0x{vmhd.GraphicsMode:X4})");

		return [
			[ "Graphics Mode", vmhd.GraphicsMode, modeDesc ],
			[ "Op Color", vmhd.OpColor, $"R={vmhd.OpColor[0]}, G={vmhd.OpColor[1]}, B={vmhd.OpColor[2]}" ],
		];
	}
}
