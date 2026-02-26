using Ufex.API;
using Ufex.API.Format;
using Ufex.API.Tree;
using Ufex.Extensions.Core.JPEG.Data;

namespace Ufex.Extensions.Core.JPEG.Structure;

/// <summary>
/// SOS - Start of Scan marker segment node.
/// Displays scan component selectors, Huffman table assignments,
/// and entropy-coded scan data information.
/// </summary>
internal class SosSegmentNode : SegmentNode
{
	public SosSegmentNode(SosSegment segment)
		: base(segment, "SOS", "Start of Scan", TreeViewIcon.Binary)
	{
	}

	protected override object[][] GetRows()
	{
		var d = (SosSegment)Segment;
		var rows = new List<object[]>();
		rows.Add(["Number of Components", d.NumberOfComponents, $"{d.NumberOfComponents} component(s) in scan"]);

		for (int i = 0; i < d.NumberOfComponents; i++)
		{
			int dc = d.GetDcTableSelector(i);
			int ac = d.GetAcTableSelector(i);
			rows.Add([$"Component {i + 1} Selector", d.ComponentSelectors[i], $"Component ID {d.ComponentSelectors[i]}"]);
			rows.Add([$"Component {i + 1} Tables", d.HuffmanTableSelectors[i], $"DC table {dc}, AC table {ac}"]);
		}

		rows.Add(["Spectral Selection Start", d.SpectralSelectionStart, $"Ss = {d.SpectralSelectionStart}"]);
		rows.Add(["Spectral Selection End", d.SpectralSelectionEnd, $"Se = {d.SpectralSelectionEnd}"]);
		rows.Add(["Successive Approximation", d.SuccessiveApproximation, $"Ah = {d.SuccessiveApproximation >> 4}, Al = {d.SuccessiveApproximation & 0x0F}"]);

		if (d.ScanDataLength > 0)
		{
			rows.Add(["Scan Data Offset", (ulong)d.ScanDataOffset, $"Offset 0x{d.ScanDataOffset:X}"]);
			rows.Add(["Scan Data Length", (ulong)d.ScanDataLength, ByteCountFormatter.Format((ulong)d.ScanDataLength)]);
		}

		return rows.ToArray();
	}
}
