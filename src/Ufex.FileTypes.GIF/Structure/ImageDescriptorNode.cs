using Ufex.API.Tree;
using Ufex.FileTypes.GIF.Data;

namespace Ufex.FileTypes.GIF.Structure;

/// <summary>
/// Image Descriptor node
/// </summary>
internal class ImageDescriptorNode : BlockNode
{
	private readonly ImageDescriptor _descriptor;

	public ImageDescriptorNode(ImageDescriptor descriptor)
		: base("Image Descriptor", TreeViewIcon.Properties, descriptor.Offset)
	{
		_descriptor = descriptor;
	}

	protected override List<object[]> GetRows()
	{
		return new List<object[]>
		{
			new object[] { "Image Separator", $"0x{_descriptor.ImageSeparator:X2}", "Always 0x2C" },
			new object[] { "Left Position", _descriptor.LeftPosition, $"{_descriptor.LeftPosition} pixels from left" },
			new object[] { "Top Position", _descriptor.TopPosition, $"{_descriptor.TopPosition} pixels from top" },
			new object[] { "Width", _descriptor.Width, $"{_descriptor.Width} pixels" },
			new object[] { "Height", _descriptor.Height, $"{_descriptor.Height} pixels" },
			new object[] { "Packed Fields", $"0x{_descriptor.PackedFields:X2}", "" },
			new object[] { "Local Color Table Flag", _descriptor.LocalColorTableFlag, _descriptor.LocalColorTableFlag ? "Local Color Table follows" : "No Local Color Table" },
			new object[] { "Interlace Flag", _descriptor.InterlaceFlag, _descriptor.InterlaceFlag ? "Interlaced" : "Not interlaced" },
			new object[] { "Sort Flag", _descriptor.SortFlag, _descriptor.SortFlag ? "Colors sorted" : "Not sorted" },
			new object[] { "Reserved", _descriptor.Reserved, "" },
			new object[] { "Size of Local Color Table", _descriptor.SizeOfLocalColorTable, $"{_descriptor.LocalColorTableSize} colors" }
		};
	}
}
