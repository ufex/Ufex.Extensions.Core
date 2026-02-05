using Ufex.API.Tree;
using Ufex.FileTypes.GIF.Data;

namespace Ufex.FileTypes.GIF.Structure;

/// <summary>
/// Frame node - contains a single image frame with its extensions
/// </summary>
internal class FrameNode : TreeNode
{
	private readonly GifFrame _frame;

	public FrameNode(GifFrame frame)
		: base($"Frame {frame.FrameIndex}", TreeViewIcon.Image, TreeViewIcon.Image)
	{
		_frame = frame;
		BuildChildren();
	}

	private void BuildChildren()
	{
		// Add Graphic Control Extension if present
		if (_frame.GraphicControlExtension != null)
		{
			Nodes.Add(new GraphicControlExtensionNode(_frame.GraphicControlExtension));
		}

		// Add Image Descriptor
		Nodes.Add(new ImageDescriptorNode(_frame.ImageDescriptor));

		// Add Local Color Table if present
		if (_frame.LocalColorTable != null)
		{
			Nodes.Add(new ColorTableNode(_frame.LocalColorTable, isGlobal: false));
		}

		// Add Image Data
		Nodes.Add(new ImageDataNode(_frame.ImageData));
	}
}
