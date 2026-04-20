using System;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ISOBMFF.Data;
using Ufex.Extensions.Core.ISOBMFF.Data.Heif;

namespace Ufex.Extensions.Core.ISOBMFF.Structure.Heif;

/// <summary>
/// infe — Item Information Entry node. Displays item ID, type, name,
/// and content type for HEIF items.
/// </summary>
internal class InfeBoxNode : BoxNode
{
	/// <summary>
	/// Known item type descriptions.
	/// </summary>
	private static readonly Dictionary<string, string> ItemTypeNames = new()
	{
		{ "hvc1", "HEVC coded image" },
		{ "hev1", "HEVC coded image (in-band params)" },
		{ "av01", "AV1 coded image" },
		{ "avc1", "AVC coded image" },
		{ "jpeg", "JPEG coded image" },
		{ "j2k1", "JPEG 2000 coded image" },
		{ "grid", "Image grid" },
		{ "iovl", "Image overlay" },
		{ "iden", "Identity derived image" },
		{ "Exif", "Exif metadata" },
		{ "mime", "MIME-typed metadata" },
		{ "uri ", "URI-typed metadata" },
		{ "hvt1", "HEVC tile image" },
		{ "av1t", "AV1 tile image" },
		{ "tmap", "Tone map (HDR gain map)" },
		{ "unci", "Uncompressed image" },
	};

	public InfeBoxNode(InfeBox box)
		: base(box, "infe", GetDescription(box), GetIcon(box))
	{
	}

	private static string GetDescription(InfeBox box)
	{
		string name = box.ItemName;
		string typeStr = box.ItemTypeString;
		if (!string.IsNullOrEmpty(typeStr))
		{
			string typeDesc = ItemTypeNames.GetValueOrDefault(typeStr, typeStr);
			return $"Item {box.ItemId}: {typeDesc}" + (name.Length > 0 ? $" ({name})" : "");
		}
		return $"Item {box.ItemId}" + (name.Length > 0 ? $" ({name})" : "");
	}

	private static TreeViewIcon GetIcon(InfeBox box)
	{
		string typeStr = box.ItemTypeString;
		return typeStr switch
		{
			"hvc1" or "hev1" or "av01" or "avc1" or "jpeg" or "j2k1" or "unci" => TreeViewIcon.Image,
			"grid" or "iovl" or "iden" => TreeViewIcon.Image,
			"Exif" or "mime" or "uri " => TreeViewIcon.Properties,
			"hvt1" or "av1t" => TreeViewIcon.Image,
			"tmap" => TreeViewIcon.Palette,
			_ => TreeViewIcon.Object,
		};
	}

	public override object[][] GetRows()
	{
		var box = (InfeBox)_box;
		var rows = new List<object[]>();

		rows.Add([ "Item ID", box.ItemId, "" ]);
		rows.Add([ "Item Protection Index", box.ItemProtectionIndex, box.ItemProtectionIndex == 0 ? "Not protected" : "" ]);

		if (box.ItemType.Length == 4)
		{
			string typeDesc = ItemTypeNames.GetValueOrDefault(box.ItemTypeString, "");
			rows.Add([ "Item Type", box.ItemType, $"{box.ItemTypeString}" + (typeDesc.Length > 0 ? $" — {typeDesc}" : "") ]);
		}

		if (box.ItemName.Length > 0)
			rows.Add([ "Item Name", box.ItemName, "" ]);

		if (box.ContentType.Length > 0)
			rows.Add([ "Content Type", box.ContentType, "" ]);

		if (box.ContentEncoding.Length > 0)
			rows.Add([ "Content Encoding", box.ContentEncoding, "" ]);

		if (box.IsHidden)
			rows.Add([ "Hidden", (Byte)1, "Item is hidden" ]);

		return rows.ToArray();
	}
}
