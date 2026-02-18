using Ufex.API.Tree;
using Ufex.Extensions.Core.GIF.Data;

namespace Ufex.Extensions.Core.GIF.Structure;

/// <summary>
/// Graphic Control Extension node
/// </summary>
internal class GraphicControlExtensionNode : BlockNode
{
	private readonly GraphicControlExtension _extension;

	public GraphicControlExtensionNode(GraphicControlExtension extension)
		: base("Graphic Control Extension", TreeViewIcon.Properties, extension.Offset)
	{
		_extension = extension;
	}

	protected override List<object[]> GetRows()
	{
		return new List<object[]>
		{
			new object[] { "Extension Introducer", $"0x{_extension.ExtensionIntroducer:X2}", "Always 0x21" },
			new object[] { "Graphic Control Label", $"0x{_extension.GraphicControlLabel:X2}", "Always 0xF9" },
			new object[] { "Block Size", _extension.BlockSize, "Always 4" },
			new object[] { "Packed Fields", $"0x{_extension.PackedFields:X2}", "" },
			new object[] { "Reserved", _extension.Reserved, "" },
			new object[] { "Disposal Method", _extension.DisposalMethod, _extension.DisposalMethodDescription },
			new object[] { "User Input Flag", _extension.UserInputFlag, _extension.UserInputFlag ? "User input expected" : "No user input" },
			new object[] { "Transparent Color Flag", _extension.TransparentColorFlag, _extension.TransparentColorFlag ? "Transparency enabled" : "No transparency" },
			new object[] { "Delay Time", _extension.DelayTime, $"{_extension.DelayTime * 10} ms" },
			new object[] { "Transparent Color Index", _extension.TransparentColorIndex, "" },
			new object[] { "Block Terminator", $"0x{_extension.BlockTerminator:X2}", "Always 0x00" }
		};
	}
}
