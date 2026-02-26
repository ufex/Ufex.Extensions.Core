using Ufex.API.Tree;
using Ufex.Extensions.Core.JPEG.Data;

namespace Ufex.Extensions.Core.JPEG.Structure;

/// <summary>
/// COM - Comment marker segment node.
/// Displays the comment text embedded in the JPEG file.
/// </summary>
internal class ComSegmentNode : SegmentNode
{
	public ComSegmentNode(ComSegment segment)
		: base(segment, "COM", "Comment", TreeViewIcon.Comment)
	{
	}

	protected override object[][] GetRows()
	{
		var d = (ComSegment)Segment;
		return [
			["Comment", d.CommentBytes, d.CommentText],
		];
	}
}
