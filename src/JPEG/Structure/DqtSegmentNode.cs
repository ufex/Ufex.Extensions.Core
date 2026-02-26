using Ufex.API.Tree;
using Ufex.Extensions.Core.JPEG.Data;

namespace Ufex.Extensions.Core.JPEG.Structure;

/// <summary>
/// DQT - Define Quantization Table marker segment node.
/// Displays quantization table metadata for each table in the segment.
/// </summary>
internal class DqtSegmentNode : SegmentNode
{
	public DqtSegmentNode(DqtSegment segment)
		: base(segment, "DQT", "Define Quantization Table", TreeViewIcon.Table)
	{
	}

	protected override object[][] GetRows()
	{
		var d = (DqtSegment)Segment;
		var rows = new List<object[]>();

		for (int i = 0; i < d.TableCount; i++)
		{
			int precision = d.GetPrecision(i);
			int tableId = d.GetTableId(i);
			string precisionDesc = precision == 0 ? "8-bit" : "16-bit";

			rows.Add([$"Table {i} Info", d.TableInfos[i], $"Precision: {precisionDesc}, ID: {tableId}"]);
			rows.Add([$"Table {i} Data", d.TableData[i], $"{d.TableData[i].Length} bytes ({precisionDesc} values)"]);
		}

		return rows.ToArray();
	}
}
