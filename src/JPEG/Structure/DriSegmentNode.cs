using Ufex.API.Tree;
using Ufex.Extensions.Core.JPEG.Data;

namespace Ufex.Extensions.Core.JPEG.Structure;

/// <summary>
/// DRI - Define Restart Interval marker segment node.
/// </summary>
internal class DriSegmentNode : SegmentNode
{
	public DriSegmentNode(DriSegment segment)
		: base(segment, "DRI", "Define Restart Interval", TreeViewIcon.Gear)
	{
	}

	protected override object[][] GetRows()
	{
		var d = (DriSegment)Segment;
		string desc = d.RestartInterval == 0
			? "Restart markers disabled"
			: $"{d.RestartInterval} MCUs between restart markers";

		return [
			["Restart Interval", d.RestartInterval, desc],
		];
	}
}
