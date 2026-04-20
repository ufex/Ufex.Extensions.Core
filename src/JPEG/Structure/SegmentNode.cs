using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.API.Format;
using Ufex.Extensions.Core.EXIF.Data;
using Ufex.Extensions.Core.JPEG.Data;

namespace Ufex.Extensions.Core.JPEG.Structure;

/// <summary>
/// Base TreeNode class for JPEG marker segments.
/// Provides common table layout with Property/Value/Description/Offset columns
/// and a factory method to create the appropriate node subclass for each segment type.
/// </summary>
internal class SegmentNode : TreeNode
{
	public Segment Segment { get; }
	public string Description { get; protected set; }

	public override Visual[] Visuals => [new DataGridVisual(TableData(), "Data")];

	protected SegmentNode(Segment segment, string typeName, string description, TreeViewIcon icon)
		: base(typeName, icon, icon)
	{
		Segment = segment;
		Description = $"{typeName} - {description}";
	}

	/// <summary>
	/// Factory method to create the appropriate node for a segment
	/// </summary>
	public static SegmentNode FromSegment(Segment segment)
	{
		return FromSegment(segment, null, null);
	}

	public static SegmentNode FromSegment(Segment segment, ExifData? exifData, long? exifSegmentOffset,
		List<Segment>? thumbnailSegments = null)
	{
		ExifData? appExifData = exifSegmentOffset == segment.Offset ? exifData : null;
		List<Segment>? appThumbSegments = appExifData != null ? thumbnailSegments : null;

		return segment switch
		{
			SoiSegment soi => new SoiSegmentNode(soi),
			EoiSegment eoi => new EoiSegmentNode(eoi),
			App0JfifSegment jfif => new App0JfifSegmentNode(jfif),
			App0JfxxSegment jfxx => new App0JfxxSegmentNode(jfxx),
			AppNSegment appn => new AppNSegmentNode(appn, appExifData, appThumbSegments),
			SofSegment sof => new SofSegmentNode(sof),
			DqtSegment dqt => new DqtSegmentNode(dqt),
			DhtSegment dht => new DhtSegmentNode(dht),
			SosSegment sos => new SosSegmentNode(sos),
			DriSegment dri => new DriSegmentNode(dri),
			ComSegment com => new ComSegmentNode(com),
			_ => new SegmentNode(segment, segment.MarkerName, "Unknown", TreeViewIcon.NullIcon),
		};
	}

	/// <summary>
	/// Builds the table data for the detail panel
	/// </summary>
	public virtual DynamicTableData TableData()
	{
		var td = new DynamicTableData(4, $"JFIF.{GetType().Name}");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");
		td.SetColumn(3, "Offset");

		// Add common marker rows
		long offset = Segment.Offset;
		td.AddRow("Marker", Segment.Marker, $"0xFF", new FileOffset(offset));
		offset += 1;
		td.AddRow("Marker Type", Segment.MarkerType, Segment.MarkerName, new FileOffset(offset));
		offset += 1;

		if (Segment.Length > 0)
		{
			td.AddRow("Length", Segment.Length, ByteCountFormatter.Format(Segment.Length), new FileOffset(offset));
			offset += 2;
		}

		// Add segment-specific rows
		foreach (var row in GetRows())
		{
			td.AddRow(row[0], row[1], row.Length > 2 ? row[2] : "", new FileOffset(offset));
			offset += ByteUtil.GetObjectSize(row[1]);
		}

		return td;
	}

	/// <summary>
	/// Override in subclasses to provide segment-specific rows.
	/// Each row is [Property, Value] or [Property, Value, Description]
	/// </summary>
	protected virtual object[][] GetRows() => [];
}
