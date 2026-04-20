using Ufex.API.Tree;
using Ufex.Extensions.Core.EXIF.Data;
using Ufex.Extensions.Core.EXIF.Structure;
using Ufex.Extensions.Core.JPEG.Data;

namespace Ufex.Extensions.Core.JPEG.Structure;

/// <summary>
/// Generic APPn marker segment node.
/// Displays the application identifier and data size.
/// </summary>
internal class AppNSegmentNode : SegmentNode
{
	public AppNSegmentNode(AppNSegment segment, ExifData? exifData = null, List<Segment>? thumbnailSegments = null)
		: base(segment, segment.MarkerName, $"Application Data ({segment.AppIdentifierString})", TreeViewIcon.Properties)
	{
		if (segment.AppIdentifierString == "Exif" && exifData != null)
		{
			TreeNode[]? thumbNodes = null;
			if (thumbnailSegments != null && thumbnailSegments.Count > 0)
			{
				thumbNodes = thumbnailSegments
					.Select(s => (TreeNode)SegmentNode.FromSegment(s))
					.ToArray();
			}

			ExifNodes.AddTo(Nodes, exifData, thumbNodes);
		}
	}

	protected override object[][] GetRows()
	{
		var d = (AppNSegment)Segment;
		return [
			["Identifier", d.AppIdentifier, d.AppIdentifierString],
			["Data", d.AppData, $"{d.AppData.Length} bytes"],
		];
	}
}
