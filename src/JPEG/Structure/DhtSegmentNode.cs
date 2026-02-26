using Ufex.API.Tree;
using Ufex.Extensions.Core.JPEG.Data;

namespace Ufex.Extensions.Core.JPEG.Structure;

/// <summary>
/// DHT - Define Huffman Table marker segment node.
/// Displays Huffman table metadata for each table in the segment.
/// </summary>
internal class DhtSegmentNode : SegmentNode
{
	public DhtSegmentNode(DhtSegment segment)
		: base(segment, "DHT", "Define Huffman Table", TreeViewIcon.Table)
	{
	}

	protected override object[][] GetRows()
	{
		var d = (DhtSegment)Segment;
		var rows = new List<object[]>();

		for (int i = 0; i < d.TableCount; i++)
		{
			int tableClass = d.GetTableClass(i);
			int tableId = d.GetTableId(i);
			string classDesc = Constants.HuffmanTableClasses.TryGetValue(tableClass, out var name) ? name : "Unknown";
			int symbolCount = d.GetSymbolCount(i);

			rows.Add([$"Table {i} Info", d.TableInfos[i], $"Class: {classDesc}, ID: {tableId}"]);
			rows.Add([$"Table {i} Code Counts", d.CodeCounts[i], "Codes per bit length (1-16)"]);
			rows.Add([$"Table {i} Symbols", d.SymbolValues[i], $"{symbolCount} symbol values"]);
		}

		return rows.ToArray();
	}
}
