using Ufex.API.Tree;
using Ufex.Extensions.Core.JPEG.Data;

namespace Ufex.Extensions.Core.JPEG.Structure;

/// <summary>
/// Generic APPn marker segment node.
/// Displays the application identifier and data size.
/// </summary>
internal class AppNSegmentNode : SegmentNode
{
	public AppNSegmentNode(AppNSegment segment)
		: base(segment, segment.MarkerName, $"Application Data ({segment.AppIdentifierString})", TreeViewIcon.Properties)
	{
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
