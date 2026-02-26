using Ufex.API.Tree;
using Ufex.Extensions.Core.JPEG.Data;

namespace Ufex.Extensions.Core.JPEG.Structure;

/// <summary>
/// APP0 JFIF - JFIF application marker segment node.
/// Displays the JFIF version, density, and thumbnail information.
/// </summary>
internal class App0JfifSegmentNode : SegmentNode
{
	public App0JfifSegmentNode(App0JfifSegment segment)
		: base(segment, "APP0 (JFIF)", "JFIF Application Data", TreeViewIcon.Properties)
	{
	}

	protected override object[][] GetRows()
	{
		var d = (App0JfifSegment)Segment;
		var rows = new List<object[]>();
		rows.Add(["Identifier", d.Identifier, "JFIF"]);
		rows.Add(["Version Major", d.VersionMajor, $"Version {d.VersionString}"]);
		rows.Add(["Version Minor", d.VersionMinor]);
		rows.Add(["Units", d.Units, Constants.GetDensityUnit(d.Units)]);
		rows.Add(["X Density", d.Xdensity, d.Xdensity.ToString()]);
		rows.Add(["Y Density", d.Ydensity, d.Ydensity.ToString()]);
		rows.Add(["X Thumbnail", d.Xthumbnail, $"{d.Xthumbnail} pixels"]);
		rows.Add(["Y Thumbnail", d.Ythumbnail, $"{d.Ythumbnail} pixels"]);

		if (d.ThumbnailData.Length > 0)
		{
			rows.Add(["Thumbnail Data", d.ThumbnailData, $"{d.ThumbnailData.Length} bytes"]);
		}

		return rows.ToArray();
	}
}
