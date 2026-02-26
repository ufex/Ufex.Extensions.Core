using Ufex.API.Tree;
using Ufex.Extensions.Core.JPEG.Data;

namespace Ufex.Extensions.Core.JPEG.Structure;

/// <summary>
/// APP0 JFXX - JFIF Extension marker segment node.
/// Displays the extension code and thumbnail information.
/// </summary>
internal class App0JfxxSegmentNode : SegmentNode
{
	public App0JfxxSegmentNode(App0JfxxSegment segment)
		: base(segment, "APP0 (JFXX)", "JFIF Extension", TreeViewIcon.Properties)
	{
	}

	protected override object[][] GetRows()
	{
		var d = (App0JfxxSegment)Segment;
		string extensionDesc = Constants.JfxxExtensionCodes.TryGetValue(d.ExtensionCode, out var desc)
			? desc
			: "Unknown";

		return [
			["Identifier", d.Identifier, "JFXX"],
			["Extension Code", d.ExtensionCode, extensionDesc],
			["Extension Data", d.ExtensionData, $"{d.ExtensionData.Length} bytes"],
		];
	}
}
