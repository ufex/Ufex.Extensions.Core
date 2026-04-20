using Microsoft.Extensions.Logging;
using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.Extensions.Core.EXIF;
using Ufex.Extensions.Core.JPEG.Data;
using Ufex.Extensions.Core.JPEG.Structure;

namespace Ufex.Extensions.Core.JPEG;

/// <summary>
/// JPEG/JFIF FileType module for Ufex.
/// Parses JPEG File Interchange Format (JFIF) files and displays their
/// internal marker segment structure, metadata, and file layout.
/// </summary>
public class JfifFileType : FileType
{
	public JfifFileType()
	{
		EnableVisual = true;
		EnableStructure = true;
		EnableValidation = true;
		Description = "JPEG/JFIF Image";
	}

	public override bool ProcessFile()
	{
		var reader = new JfifStreamReader(FileInStream, Log, ValidationReport);
		bool result = reader.Read();

		if (!result)
			return false;

		BuildQuickInfo(reader);
		BuildVisuals(reader);
		BuildStructure(reader);

		return true;
	}

	private void BuildQuickInfo(JfifStreamReader reader)
	{
		// Image dimensions from SOF
		if (reader.Sof != null)
		{
			QuickInfoTable.AddRow("Width", $"{reader.Sof.SamplesPerLine} pixels");
			QuickInfoTable.AddRow("Height", $"{reader.Sof.NumberOfLines} pixels");
			QuickInfoTable.AddRow("Sample Precision", $"{reader.Sof.SamplePrecision} bits");
			QuickInfoTable.AddRow("Components", reader.Sof.NumberOfComponents.ToString());
			QuickInfoTable.AddRow("Compression", Constants.GetSofType(reader.Sof.MarkerType));
		}

		// JFIF version and density from APP0
		if (reader.JfifApp0 != null)
		{
			var jfif = reader.JfifApp0;
			QuickInfoTable.AddRow("JFIF Version", jfif.VersionString);
			QuickInfoTable.AddRow("Density Units", Constants.GetDensityUnit(jfif.Units));
			QuickInfoTable.AddRow("X Density", jfif.Xdensity.ToString());
			QuickInfoTable.AddRow("Y Density", jfif.Ydensity.ToString());

			if (jfif.Xthumbnail > 0 && jfif.Ythumbnail > 0)
			{
				QuickInfoTable.AddRow("Thumbnail", $"{jfif.Xthumbnail} x {jfif.Ythumbnail}");
			}
		}

		// Component subsampling info
		if (reader.Sof != null && reader.Sof.NumberOfComponents == 3)
		{
			int h0 = reader.Sof.GetHorizontalSampling(0);
			int v0 = reader.Sof.GetVerticalSampling(0);
			int h1 = reader.Sof.GetHorizontalSampling(1);
			int v1 = reader.Sof.GetVerticalSampling(1);

			string subsampling = (h0, v0, h1, v1) switch
			{
				(2, 2, 1, 1) => "4:2:0",
				(2, 1, 1, 1) => "4:2:2",
				(1, 1, 1, 1) => "4:4:4",
				(4, 1, 1, 1) => "4:1:1",
				_ => $"{h0}x{v0} / {h1}x{v1}",
			};
			QuickInfoTable.AddRow("Chroma Subsampling", subsampling);
		}

		// Comments
		foreach (var segment in reader.Segments)
		{
			if (segment is ComSegment com && com.CommentText.Length > 0)
			{
				string commentText = com.CommentText;
				if (commentText.Length > 50)
					commentText = commentText[..47] + "...";
				QuickInfoTable.AddRow("Comment", commentText);
				break; // Only show the first comment in quick info
			}
		}

		QuickInfoTable.AddRow("Segments", reader.Segments.Count.ToString());

		if (reader.ExifData != null)
			ExifQuickInfo.Populate(QuickInfoTable, reader.ExifData);
	}

	private void BuildVisuals(JfifStreamReader reader)
	{
		var spans = new List<FileSpan>();

		foreach (var segment in reader.Segments)
		{
			var node = SegmentNode.FromSegment(segment);

			long endPosition = segment.Offset + segment.TotalSize;

			// For SOS segments, include the entropy-coded scan data
			if (segment is SosSegment sos && sos.ScanDataLength > 0)
			{
				endPosition = sos.ScanDataOffset + sos.ScanDataLength;
			}

			spans.Add(new FileSpan
			{
				StartPosition = segment.Offset,
				EndPosition = endPosition,
				Name = node.Description,
			});
		}

		foreach (var unknown in reader.UnknownDataSegments)
		{
			spans.Add(new FileSpan
			{
				StartPosition = unknown.Offset,
				EndPosition = unknown.Offset + unknown.DataLength,
				Name = "Unknown - Unrecognized Data",
			});
		}

		var map = new FileMap(spans.ToArray(), (ulong)FileInStream.Length);
		VisualsList.Add(map);
	}

	private void BuildStructure(JfifStreamReader reader)
	{
		// Merge segments and unknown data regions in file offset order
		int segIdx = 0;
		int unkIdx = 0;
		var segments = reader.Segments;
		var unknowns = reader.UnknownDataSegments;

		while (segIdx < segments.Count || unkIdx < unknowns.Count)
		{
			bool useSegment;
			if (segIdx >= segments.Count)
				useSegment = false;
			else if (unkIdx >= unknowns.Count)
				useSegment = true;
			else
				useSegment = segments[segIdx].Offset <= unknowns[unkIdx].Offset;

			if (useSegment)
			{
				var segment = segments[segIdx++];
				Log.LogInformation($"Processing segment {segment.MarkerName} at offset {segment.Offset}");
				var node = SegmentNode.FromSegment(segment, reader.ExifData, reader.ExifSegmentOffset,
					reader.ThumbnailSegments);
				TreeNodes.Add(node);
			}
			else
			{
				var unknown = unknowns[unkIdx++];
				Log.LogInformation($"Processing unknown data at offset {unknown.Offset}");
				var node = new UnknownDataSegmentNode(unknown);
				TreeNodes.Add(node);
			}
		}
	}
}
