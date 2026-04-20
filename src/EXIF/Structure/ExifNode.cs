using Ufex.API.Tables;
using Ufex.API.Tree;
using Ufex.API.Visual;
using Ufex.Extensions.Core.EXIF.Data;

namespace Ufex.Extensions.Core.EXIF.Structure;

/// <summary>
/// Utility class for building EXIF tree nodes.
/// </summary>
public static class ExifNodes
{
	/// <summary>
	/// Adds EXIF child nodes (TIFF Header, IFD0, ExifIFD, GPSIFD, IFD1) to the given parent node.
	/// </summary>
	public static void AddTo(TreeNodeCollection nodes, ExifData exifData, TreeNode[]? thumbnailNodes = null)
	{
		nodes.Add(new TiffHeaderNode(exifData.TiffHeader));
		nodes.Add(new IfdNode(exifData.Ifd0, exifData));
		if (exifData.ExifIfd != null)
			nodes.Add(new IfdNode(exifData.ExifIfd, exifData));
		if (exifData.GpsIfd != null)
			nodes.Add(new IfdNode(exifData.GpsIfd, exifData));
		if (exifData.Ifd1 != null)
			nodes.Add(new IfdNode(exifData.Ifd1, exifData, thumbnailNodes));
	}
}
