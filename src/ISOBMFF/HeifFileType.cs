using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Tree;
using Ufex.API.Visual;

using Ufex.Extensions.Core.EXIF;
using Ufex.Extensions.Core.EXIF.Data;
using Ufex.Extensions.Core.EXIF.Structure;

using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.Heif;
using Ufex.Extensions.Core.ISOBMFF.Structure;

namespace Ufex.Extensions.Core.ISOBMFF;

/// <summary>
/// FileType handler for HEIF/HEIC/AVIF image files.
/// HEIF files use ISOBMFF's item infrastructure (meta, iloc, iinf, iprp)
/// rather than the track-based moov hierarchy used by MP4/3GPP.
/// </summary>
public class HeifFileType : BaseIsobmffFileType
{
	public HeifFileType()
	{
	}

	protected override void BuildQuickInfo(BoxStreamReader reader)
	{
		var quickInfo = QuickInfoTable;

		// Brand info from ftyp
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

		// Primary item dimensions from ispe
		var ispe = FindPrimaryImageIspe(reader.Boxes);
		if (ispe != null)
		{
			quickInfo.AddRow("Width", $"{ispe.ImageWidth} pixels");
			quickInfo.AddRow("Height", $"{ispe.ImageHeight} pixels");
		}

		// Primary image codec from infe
		string? primaryCodec = FindPrimaryImageCodec(reader.Boxes);
		if (primaryCodec != null)
			quickInfo.AddRow("Image Codec", primaryCodec);

		// Pixel info from pixi
		var pixi = FindBoxInIpco<PixiBox>(reader.Boxes);
		if (pixi != null)
		{
			string bpcStr = string.Join(", ", pixi.BitsPerChannel.Select(b => $"{b}"));
			quickInfo.AddRow("Bits Per Channel", bpcStr);
			quickInfo.AddRow("Channels", pixi.NumChannels.ToString());
		}

		// Colour info from colr
		var colr = FindBoxInIpco<ColrBox>(reader.Boxes);
		if (colr != null)
		{
			string colourTypeStr = colr.ColourTypeString;
			if (colourTypeStr == "nclx")
			{
				string primaries = ColrBox.ColourPrimariesNames.GetValueOrDefault(colr.ColourPrimaries, $"Code {colr.ColourPrimaries}");
				quickInfo.AddRow("Colour Space", primaries);
				quickInfo.AddRow("Full Range", colr.FullRangeFlag ? "Yes" : "No");
			}
			else if (colourTypeStr == "rICC" || colourTypeStr == "prof")
			{
				quickInfo.AddRow("Colour Profile", $"ICC profile ({colr.IccProfileLength} bytes)");
			}
		}

		// Rotation from irot
		var irot = FindBoxInIpco<IrotBox>(reader.Boxes);
		if (irot != null && irot.AngleDegrees != 0)
			quickInfo.AddRow("Rotation", $"{irot.AngleDegrees}°");

		// Item count
		Int32 itemCount = CountItems(reader.Boxes);
		if (itemCount > 0)
			quickInfo.AddRow("Items", itemCount.ToString());

		// EXIF metadata
		ExifData? exifData = TryReadExif(reader);
		if (exifData != null)
			ExifQuickInfo.Populate(quickInfo, exifData);
	}

	protected override void BuildStructure(BoxStreamReader reader)
	{
		ExifData? exifData = TryReadExif(reader);

		foreach (Box box in reader.Boxes)
		{
			BoxNode node = BoxNode.FromBox(box);

			// Attach EXIF node under the meta box if Exif data was found
			if (exifData != null && box.TypeString == "meta")
			{
				ExifNodes.AddTo(node.Nodes, exifData);
			}

			TreeNodes.Add(node);
		}
	}

	protected override void ValidateFile(BoxStreamReader reader)
	{
		// ftyp must be present and first
		var ftyp = FindBox<FtypBox>(reader.Boxes);
		if (ftyp == null)
		{
			ValidationReport.Warning("No 'ftyp' box found. HEIF files must begin with a File Type box.");
		}
		else
		{
			var firstBox = reader.Boxes.FirstOrDefault();
			if (firstBox != null && firstBox.TypeString != "ftyp")
				ValidationReport.Warning($"First box is '{firstBox.TypeString.Trim()}', not 'ftyp'. HEIF files must have 'ftyp' as the first box.");
		}

		// meta must be present at file level
		bool hasMeta = reader.Boxes.Any(b => b.TypeString == "meta");
		if (!hasMeta)
			ValidationReport.Warning("No file-level 'meta' box found. HEIF files require a 'meta' box containing item infrastructure.");

		// Check handler type inside meta
		if (hasMeta)
		{
			var metaBox = reader.Boxes.First(b => b.TypeString == "meta");
			var hdlr = FindBoxInChildren<HdlrBox>(metaBox);
			if (hdlr != null && hdlr.HandlerTypeString != "pict")
				ValidationReport.Info($"Handler type in meta is '{hdlr.HandlerTypeString}', expected 'pict' for HEIF image files.");

			// pitm should be present
			var pitm = FindBoxInChildren<PitmBox>(metaBox);
			if (pitm == null)
				ValidationReport.Info("No 'pitm' (Primary Item) box found in meta. HEIF files should identify the primary image.");

			// iloc should be present
			var iloc = FindBoxInChildren<IlocBox>(metaBox);
			if (iloc == null)
				ValidationReport.Info("No 'iloc' (Item Location) box found in meta. HEIF files need iloc to locate item data.");

			// iinf should be present
			bool hasIinf = metaBox.Children.Any(b => b.TypeString == "iinf");
			if (!hasIinf)
				ValidationReport.Info("No 'iinf' (Item Information) box found in meta.");
		}
	}

	/// <summary>
	/// Attempts to find and parse EXIF data from the HEIF file.
	/// HEIF stores Exif as an item with type 'Exif' in infe, with data located via iloc.
	/// The payload has a 4-byte exif_tiff_header_offset prefix before the TIFF header.
	/// </summary>
	private ExifData? TryReadExif(BoxStreamReader reader)
	{
		// Find meta box
		var metaBox = reader.Boxes.FirstOrDefault(b => b.TypeString == "meta");
		if (metaBox == null) return null;

		// Find infe entries to locate Exif item
		UInt32? exifItemId = null;
		foreach (var child in metaBox.Children)
		{
			if (child.TypeString == "iinf")
			{
				foreach (var infe in child.Children)
				{
					if (infe is InfeBox infeBox && infeBox.ItemTypeString == "Exif")
					{
						exifItemId = infeBox.ItemId;
						break;
					}
				}
			}
		}

		if (exifItemId == null)
		{
			Log.Debug("HEIF: No Exif item found in iinf entries.");
			return null;
		}

		// Find iloc to get the offset and length
		var iloc = FindBoxInChildren<IlocBox>(metaBox);
		if (iloc == null)
		{
			Log.Debug("HEIF: No iloc box found in meta.");
			return null;
		}

		IlocItem? exifItem = null;
		foreach (var item in iloc.Items)
		{
			if (item.ItemId == exifItemId)
			{
				exifItem = item;
				break;
			}
		}

		if (exifItem == null || exifItem.Value.Extents.Length == 0)
		{
			Log.Debug($"HEIF: Exif item ID {exifItemId} not found in iloc, or has no extents.");
			return null;
		}

		var extent = exifItem.Value.Extents[0];
		Int64 dataOffset = (Int64)(exifItem.Value.BaseOffset + extent.ExtentOffset);
		Int64 dataLength = (Int64)extent.ExtentLength;

		Log.Debug($"HEIF: Exif item at offset {dataOffset}, length {dataLength}");

		if (dataLength < 12) return null; // 4-byte prefix + minimum TIFF header
		if (dataOffset + dataLength > FileInStream.Length) return null;

		// Read the 4-byte exif_tiff_header_offset prefix
		// Per ISO 14496-12: payload = [4-byte offset] [offset bytes padding] [TIFF data]
		FileInStream.Seek(dataOffset, SeekOrigin.Begin);
		var fr = new FileReader(FileInStream, Endian.Big);
		UInt32 tiffHeaderOffset = fr.ReadUInt32();

		Int64 tiffStart = dataOffset + 4 + tiffHeaderOffset;
		Int64 tiffLength = dataLength - 4 - tiffHeaderOffset;

		Log.Debug($"HEIF: TIFF header offset prefix = {tiffHeaderOffset}, tiffStart = {tiffStart}, tiffLength = {tiffLength}");

		if (tiffLength < 8) return null;

		var exifReader = new ExifStreamReader(FileInStream, Log, ValidationReport, tiffStart, tiffLength);
		if (exifReader.Read())
			return exifReader.ExifData;

		return null;
	}

	/// <summary>
	/// Finds the ispe box for the primary image item.
	/// </summary>
	private IspeBox? FindPrimaryImageIspe(List<Box> boxes)
	{
		// Simple approach: find the first ispe in ipco
		return FindBoxInIpco<IspeBox>(boxes);
	}

	/// <summary>
	/// Gets the codec description for the primary image item from infe.
	/// </summary>
	private string? FindPrimaryImageCodec(List<Box> boxes)
	{
		var metaBox = boxes.FirstOrDefault(b => b.TypeString == "meta");
		if (metaBox == null) return null;

		var pitm = FindBoxInChildren<PitmBox>(metaBox);
		UInt32? primaryId = pitm?.ItemId;

		foreach (var child in metaBox.Children)
		{
			if (child.TypeString == "iinf")
			{
				foreach (var infe in child.Children)
				{
					if (infe is InfeBox infeBox)
					{
						bool isPrimary = primaryId == null || infeBox.ItemId == primaryId;
						if (isPrimary && infeBox.ItemType.Length == 4)
						{
							return infeBox.ItemTypeString switch
							{
								"hvc1" or "hev1" => "HEVC",
								"av01" => "AV1",
								"avc1" => "AVC / H.264",
								"jpeg" => "JPEG",
								"j2k1" => "JPEG 2000",
								"unci" => "Uncompressed",
								"grid" => "Image Grid",
								_ => infeBox.ItemTypeString,
							};
						}
					}
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Counts total items from iinf entries.
	/// </summary>
	private Int32 CountItems(List<Box> boxes)
	{
		var metaBox = boxes.FirstOrDefault(b => b.TypeString == "meta");
		if (metaBox == null) return 0;

		foreach (var child in metaBox.Children)
		{
			if (child.TypeString == "iinf")
				return child.Children.Count;
		}

		return 0;
	}

	/// <summary>
	/// Finds a box of type T inside the ipco container within meta → iprp → ipco.
	/// </summary>
	private T? FindBoxInIpco<T>(List<Box> boxes) where T : Box
	{
		var metaBox = boxes.FirstOrDefault(b => b.TypeString == "meta");
		if (metaBox == null) return null;

		foreach (var child in metaBox.Children)
		{
			if (child.TypeString == "iprp")
			{
				foreach (var ipcoChild in child.Children)
				{
					if (ipcoChild.TypeString == "ipco")
					{
						foreach (var prop in ipcoChild.Children)
						{
							if (prop is T found)
								return found;
						}
					}
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Finds a box of type T in the direct children of the given box.
	/// </summary>
	private T? FindBoxInChildren<T>(Box parent) where T : Box
	{
		foreach (var child in parent.Children)
		{
			if (child is T found)
				return found;
		}
		return null;
	}
}
