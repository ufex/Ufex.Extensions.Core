using Ufex.API.Tree;
using Ufex.Extensions.Core.GIF.Data;

namespace Ufex.Extensions.Core.GIF.Structure;

/// <summary>
/// Table Based Image Data node
/// </summary>
internal class ImageDataNode : BlockNode
{
	private readonly TableBasedImageData _imageData;

	public ImageDataNode(TableBasedImageData imageData)
		: base("Image Data", TreeViewIcon.Image, imageData.Offset)
	{
		_imageData = imageData;
	}

	protected override List<object[]> GetRows()
	{
		return new List<object[]>
		{
			new object[] { "LZW Minimum Code Size", _imageData.LzwMinimumCodeSize, $"Initial code size: {_imageData.LzwMinimumCodeSize + 1} bits" },
			new object[] { "Data Blocks", _imageData.BlockCount, $"{_imageData.BlockCount} sub-blocks" },
			new object[] { "Total Data Size", _imageData.TotalDataSize, $"{_imageData.TotalDataSize} bytes compressed" }
		};
	}
}
