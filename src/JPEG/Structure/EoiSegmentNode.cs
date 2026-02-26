using Ufex.API.Tree;
using Ufex.Extensions.Core.JPEG.Data;

namespace Ufex.Extensions.Core.JPEG.Structure;

/// <summary>
/// EOI - End of Image marker node
/// </summary>
internal class EoiSegmentNode : SegmentNode
{
	public EoiSegmentNode(EoiSegment segment)
		: base(segment, "EOI", "End of Image", TreeViewIcon.Section)
	{
	}
}
