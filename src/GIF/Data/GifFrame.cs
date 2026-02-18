namespace Ufex.Extensions.Core.GIF.Data;

/// <summary>
/// Represents a single image (frame) within a GIF file
/// Contains optional extensions, the image descriptor, optional local color table,
/// and the compressed image data
/// </summary>
internal class GifFrame
{
	/// <summary>
	/// Optional Graphic Control Extension (GIF89a only)
	/// </summary>
	public GraphicControlExtension? GraphicControlExtension { get; set; }

	/// <summary>
	/// Image Descriptor (required)
	/// </summary>
	public ImageDescriptor ImageDescriptor { get; set; } = null!;

	/// <summary>
	/// Optional Local Color Table
	/// </summary>
	public ColorTable? LocalColorTable { get; set; }

	/// <summary>
	/// Table Based Image Data (required)
	/// </summary>
	public TableBasedImageData ImageData { get; set; } = null!;

	/// <summary>
	/// Frame index (0-based)
	/// </summary>
	public int FrameIndex { get; set; }

	/// <summary>
	/// Get the delay time in milliseconds
	/// </summary>
	public int DelayTimeMs => (GraphicControlExtension?.DelayTime ?? 0) * 10;

	/// <summary>
	/// Whether this frame has transparency
	/// </summary>
	public bool HasTransparency => GraphicControlExtension?.TransparentColorFlag ?? false;
}
