using Ufex.API.Tree;
using Ufex.Extensions.Core.GIF.Data;

namespace Ufex.Extensions.Core.GIF.Structure;

/// <summary>
/// Logical Screen Descriptor node
/// </summary>
internal class LogicalScreenDescriptorNode : BlockNode
{
	private readonly LogicalScreenDescriptor _descriptor;

	public LogicalScreenDescriptorNode(LogicalScreenDescriptor descriptor)
		: base("Logical Screen Descriptor", TreeViewIcon.Properties, descriptor.Offset)
	{
		_descriptor = descriptor;
	}

	protected override List<object[]> GetRows()
	{
		return new List<object[]>
		{
			new object[] { "Width", _descriptor.Width, $"{_descriptor.Width} pixels" },
			new object[] { "Height", _descriptor.Height, $"{_descriptor.Height} pixels" },
			new object[] { "Packed Fields", $"0x{_descriptor.PackedFields:X2}", "" },
			new object[] { "Global Color Table Flag", _descriptor.GlobalColorTableFlag, _descriptor.GlobalColorTableFlag ? "Global Color Table follows" : "No Global Color Table" },
			new object[] { "Color Resolution", _descriptor.ColorResolution, $"{_descriptor.ColorResolution} bits per primary color" },
			new object[] { "Sort Flag", _descriptor.SortFlag, _descriptor.SortFlag ? "Colors sorted by importance" : "Not sorted" },
			new object[] { "Size of Global Color Table", _descriptor.SizeOfGlobalColorTable, $"{_descriptor.GlobalColorTableSize} colors" },
			new object[] { "Background Color Index", _descriptor.BackgroundColorIndex, "" },
			new object[] { "Pixel Aspect Ratio", _descriptor.PixelAspectRatio, _descriptor.PixelAspectRatio == 0 ? "Square pixels" : $"Ratio: {(_descriptor.PixelAspectRatio + 15) / 64.0:F2}" }
		};
	}
}
