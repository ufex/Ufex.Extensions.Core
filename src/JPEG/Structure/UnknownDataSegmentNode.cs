using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Tree;
using Ufex.API.Visual;
using Ufex.API.Format;
using Ufex.Extensions.Core.JPEG.Data;

namespace Ufex.Extensions.Core.JPEG.Structure;

/// <summary>
/// Tree node for unknown/unrecognized data regions in the JPEG file.
/// </summary>
internal class UnknownDataSegmentNode : TreeNode
{
	public UnknownDataSegment UnknownData { get; }
	public string Description { get; }

	public override Visual[] Visuals => [new DataGridVisual(TableData(), "Data")];

	public UnknownDataSegmentNode(UnknownDataSegment unknownData)
		: base("Unknown", TreeViewIcon.NullIcon, TreeViewIcon.NullIcon)
	{
		UnknownData = unknownData;
		Description = "Unknown - Unrecognized Data";
	}

	public DynamicTableData TableData()
	{
		var td = new DynamicTableData(4, "JFIF.UnknownData");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");
		td.SetColumn(3, "Offset");

		td.AddRow("Offset", UnknownData.Offset, $"0x{UnknownData.Offset:X}", new FileOffset(UnknownData.Offset));
		td.AddRow("Length", (ulong)UnknownData.DataLength, ByteCountFormatter.Format((ulong)UnknownData.DataLength), new FileOffset(UnknownData.Offset));

		return td;
	}
}
