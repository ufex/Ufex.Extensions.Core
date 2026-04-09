using System;
using System.Collections.Generic;
using System.IO;

using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Tree;
using Ufex.API.Visual;

using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Structure;

namespace Ufex.Extensions.Core.ISOBMFF;

/// <summary>
/// Base FileType for QTFF/ISOBMFF container formats. Contains shared parsing
/// and display logic. Subclassed by QtffFileType and IsoBmffFileType for
/// format-specific validation and presentation.
/// </summary>
public abstract class BaseIsobmffFileType : FileType
{
	protected FileMap? Map { get; set; }

	protected BaseIsobmffFileType()
	{
		EnableVisual = true;
		EnableStructure = true;
		EnableValidation = true;
	}

	public override bool ProcessFile()
	{
		BoxStreamReader reader = new BoxStreamReader(FileInStream, Log, ValidationReport);
		bool result = reader.Read();

		BuildQuickInfo(reader);
		BuildVisuals(reader);
		BuildStructure(reader);
		ValidateFile(reader);

		return result;
	}

	protected virtual void BuildQuickInfo(BoxStreamReader reader)
	{
		var quickInfo = QuickInfoTable;

		// Find ftyp box if present
		var ftypBox = FindBox<FtypBox>(reader.Boxes);
		if (ftypBox != null)
		{
			quickInfo.AddRow("Major Brand", ftypBox.MajorBrandString);
			string brandDesc = BoxTypes.Brands.GetValueOrDefault(ftypBox.MajorBrandString, "");
			if (!string.IsNullOrEmpty(brandDesc))
				quickInfo.AddRow("Format", brandDesc);

			var brands = new List<string>();
			foreach (var brand in ftypBox.CompatibleBrands)
				brands.Add(System.Text.Encoding.ASCII.GetString(brand).Trim());
			if (brands.Count > 0)
				quickInfo.AddRow("Compatible Brands", string.Join(", ", brands));
		}

		// Find mvhd for duration/timescale
		var mvhdBox = FindBox<MvhdBox>(reader.Boxes);
		if (mvhdBox != null)
		{
			double durationSec = mvhdBox.Timescale > 0 ? (double)mvhdBox.Duration / mvhdBox.Timescale : 0;
			quickInfo.AddRow("Duration", $"{durationSec:F3} seconds");
			quickInfo.AddRow("Timescale", $"{mvhdBox.Timescale} ticks/sec");
			quickInfo.AddRow("Next Track ID", mvhdBox.NextTrackID.ToString());
		}

		// Count tracks
		int trackCount = 0;
		foreach (var box in reader.Boxes)
		{
			if (box.TypeString == "moov")
			{
				foreach (var child in box.Children)
				{
					if (child.TypeString == "trak")
						trackCount++;
				}
			}
		}
		if (trackCount > 0)
			quickInfo.AddRow("Tracks", trackCount.ToString());
	}

	protected virtual void BuildVisuals(BoxStreamReader reader)
	{
		var spans = new List<FileSpan>();

		foreach (Box box in reader.Boxes)
		{
			string typeStr = box.TypeString.Trim();
			string description = BoxTypes.Descriptions.GetValueOrDefault(box.TypeString, typeStr);
			spans.Add(new FileSpan
			{
				StartPosition = box.Offset,
				EndPosition = box.Offset + (long)box.ActualSize,
				Name = $"{typeStr} — {description}"
			});
		}

		Map = new FileMap(spans.ToArray(), (ulong)FileInStream.Length);
		VisualsList.Add(Map);
	}

	protected virtual void BuildStructure(BoxStreamReader reader)
	{
		foreach (Box box in reader.Boxes)
		{
			BoxNode node = BoxNode.FromBox(box);
			TreeNodes.Add(node);
		}
	}

	/// <summary>
	/// Override in subclasses to perform format-specific validation.
	/// </summary>
	protected virtual void ValidateFile(BoxStreamReader reader)
	{
	}

	/// <summary>
	/// Recursively search for a box of a specific type in the box tree.
	/// </summary>
	private protected T? FindBox<T>(List<Box> boxes) where T : Box
	{
		foreach (var box in boxes)
		{
			if (box is T typed)
				return typed;
			var found = FindBox<T>(box.Children);
			if (found != null)
				return found;
		}
		return null;
	}
}
