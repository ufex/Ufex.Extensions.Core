using Ufex.API.Tree;
using Ufex.Extensions.Core.JPEG.Data;

namespace Ufex.Extensions.Core.JPEG.Structure;

/// <summary>
/// SOI - Start of Image marker node
/// </summary>
internal class SoiSegmentNode : SegmentNode
{
	public SoiSegmentNode(SoiSegment segment)
		: base(segment, "SOI", "Start of Image", TreeViewIcon.Header)
	{
	}
}
