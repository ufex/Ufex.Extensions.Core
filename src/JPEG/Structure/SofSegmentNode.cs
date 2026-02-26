using Ufex.API.Tree;
using Ufex.Extensions.Core.JPEG.Data;

namespace Ufex.Extensions.Core.JPEG.Structure;

/// <summary>
/// SOFn - Start of Frame marker segment node.
/// Displays image dimensions, sample precision, and per-component information.
/// </summary>
internal class SofSegmentNode : SegmentNode
{
	public SofSegmentNode(SofSegment segment)
		: base(segment, segment.MarkerName, Constants.GetSofType(segment.MarkerType), TreeViewIcon.Image)
	{
	}

	protected override object[][] GetRows()
	{
		var d = (SofSegment)Segment;
		var rows = new List<object[]>();
		rows.Add(["Sample Precision", d.SamplePrecision, $"{d.SamplePrecision} bits"]);
		rows.Add(["Number of Lines", d.NumberOfLines, $"{d.NumberOfLines} pixels (height)"]);
		rows.Add(["Samples per Line", d.SamplesPerLine, $"{d.SamplesPerLine} pixels (width)"]);
		rows.Add(["Number of Components", d.NumberOfComponents, d.NumberOfComponents == 1 ? "Grayscale" : d.NumberOfComponents == 3 ? "YCbCr" : $"{d.NumberOfComponents} components"]);

		for (int i = 0; i < d.NumberOfComponents; i++)
		{
			string compName = SofSegment.GetComponentName(d.ComponentIds[i], i);
			int h = d.GetHorizontalSampling(i);
			int v = d.GetVerticalSampling(i);
			rows.Add([$"Component {i + 1} ID", d.ComponentIds[i], compName]);
			rows.Add([$"Component {i + 1} Sampling", d.SamplingFactors[i], $"{h}x{v}"]);
			rows.Add([$"Component {i + 1} QT Selector", d.QuantizationTableSelectors[i], $"Table {d.QuantizationTableSelectors[i]}"]);
		}

		return rows.ToArray();
	}
}
