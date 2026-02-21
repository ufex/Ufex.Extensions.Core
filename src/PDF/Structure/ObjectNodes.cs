using Ufex.API.Tree;
using Ufex.Extensions.Core.PDF.Data;

namespace Ufex.Extensions.Core.PDF.Structure;

/// <summary>
/// Node for the Document Catalog (/Type /Catalog)
/// </summary>
internal class CatalogNode : PdfObjectNode
{
	public CatalogNode(IndirectObject obj)
		: base(obj, $"Catalog ({obj.ObjectNumber} 0)", "Document Catalog", TreeViewIcon.Document)
	{
	}

	public override object[][] GetRows()
	{
		var dict = Obj.Dictionary;
		if (dict == null) return [];

		var rows = new List<object[]>();
		if (dict.ContainsKey("Pages"))
			rows.Add(["/Pages", FormatValue(dict.Get("Pages")), "Root of page tree"]);
		if (dict.ContainsKey("PageLayout"))
			rows.Add(["/PageLayout", FormatValue(dict.Get("PageLayout")), "Page layout mode"]);
		if (dict.ContainsKey("PageMode"))
			rows.Add(["/PageMode", FormatValue(dict.Get("PageMode")), "Page display mode"]);
		if (dict.ContainsKey("Outlines"))
			rows.Add(["/Outlines", FormatValue(dict.Get("Outlines")), "Document outline (bookmarks)"]);
		if (dict.ContainsKey("Metadata"))
			rows.Add(["/Metadata", FormatValue(dict.Get("Metadata")), "XMP metadata stream"]);
		if (dict.ContainsKey("MarkInfo"))
			rows.Add(["/MarkInfo", FormatValue(dict.Get("MarkInfo")), "Marked content info"]);
		if (dict.ContainsKey("Lang"))
			rows.Add(["/Lang", FormatValue(dict.Get("Lang")), "Document language"]);
		if (dict.ContainsKey("AcroForm"))
			rows.Add(["/AcroForm", FormatValue(dict.Get("AcroForm")), "Interactive form dictionary"]);
		if (dict.ContainsKey("Version"))
			rows.Add(["/Version", FormatValue(dict.Get("Version")), "PDF version override"]);

		// Add remaining entries
		rows.AddRange(DictEntryRows(dict, "Type", "Pages", "PageLayout", "PageMode",
			"Outlines", "Metadata", "MarkInfo", "Lang", "AcroForm", "Version"));

		return rows.ToArray();
	}
}

/// <summary>
/// Node for the Pages tree root (/Type /Pages)
/// </summary>
internal class PagesNode : PdfObjectNode
{
	public PagesNode(IndirectObject obj)
		: base(obj, $"Pages ({obj.ObjectNumber} 0)", "Page Tree Root", TreeViewIcon.FolderOpen)
	{
	}

	public override object[][] GetRows()
	{
		var dict = Obj.Dictionary;
		if (dict == null) return [];

		var rows = new List<object[]>();
		if (dict.ContainsKey("Count"))
			rows.Add(["/Count", FormatValue(dict.Get("Count")), "Total number of pages"]);
		if (dict.ContainsKey("Kids"))
		{
			var kids = dict.GetArray("Kids");
			rows.Add(["/Kids", kids != null ? $"[{kids.Count} items]" : "[]", "Child page nodes"]);
		}
		if (dict.ContainsKey("MediaBox"))
			rows.Add(["/MediaBox", FormatValue(dict.Get("MediaBox")), "Default media box"]);
		if (dict.ContainsKey("Resources"))
			rows.Add(["/Resources", FormatValue(dict.Get("Resources")), "Inherited resources"]);

		rows.AddRange(DictEntryRows(dict, "Type", "Count", "Kids", "MediaBox", "Resources"));
		return rows.ToArray();
	}
}

/// <summary>
/// Node for a Page object (/Type /Page)
/// </summary>
internal class PageNode : PdfObjectNode
{
	public PageNode(IndirectObject obj)
		: base(obj, $"Page ({obj.ObjectNumber} 0)", "Page", TreeViewIcon.Document)
	{
	}

	public override object[][] GetRows()
	{
		var dict = Obj.Dictionary;
		if (dict == null) return [];

		var rows = new List<object[]>();
		if (dict.ContainsKey("Parent"))
			rows.Add(["/Parent", FormatValue(dict.Get("Parent")), "Parent page tree node"]);
		if (dict.ContainsKey("MediaBox"))
			rows.Add(["/MediaBox", FormatMediaBox(dict.GetArray("MediaBox")), "Page boundaries"]);
		if (dict.ContainsKey("CropBox"))
			rows.Add(["/CropBox", FormatMediaBox(dict.GetArray("CropBox")), "Visible region"]);
		if (dict.ContainsKey("Rotate"))
			rows.Add(["/Rotate", FormatValue(dict.Get("Rotate")), "Page rotation (degrees)"]);
		if (dict.ContainsKey("Contents"))
			rows.Add(["/Contents", FormatValue(dict.Get("Contents")), "Content stream(s)"]);
		if (dict.ContainsKey("Resources"))
			rows.Add(["/Resources", FormatValue(dict.Get("Resources")), "Page resources"]);
		if (dict.ContainsKey("Annots"))
		{
			var annots = dict.GetArray("Annots");
			rows.Add(["/Annots", annots != null ? $"[{annots.Count} annotations]" : FormatValue(dict.Get("Annots")), "Page annotations"]);
		}

		rows.AddRange(DictEntryRows(dict, "Type", "Parent", "MediaBox", "CropBox", "Rotate", "Contents", "Resources", "Annots"));
		return rows.ToArray();
	}

	private static string FormatMediaBox(PdfArray? box)
	{
		if (box == null || box.Count < 4) return "";
		return $"[{FormatNum(box[0])} {FormatNum(box[1])} {FormatNum(box[2])} {FormatNum(box[3])}]";
	}

	private static string FormatNum(PdfObject obj) => obj switch
	{
		PdfInteger i => i.Value.ToString(),
		PdfReal r => r.Value.ToString("G"),
		_ => obj.ToString() ?? ""
	};
}

/// <summary>
/// Node for a Font object (/Type /Font)
/// </summary>
internal class FontNode : PdfObjectNode
{
	public FontNode(IndirectObject obj)
		: base(obj, $"Font ({obj.ObjectNumber} 0)", "Font", TreeViewIcon.Text)
	{
	}

	public override object[][] GetRows()
	{
		var dict = Obj.Dictionary;
		if (dict == null) return [];

		var rows = new List<object[]>();
		if (dict.ContainsKey("Subtype"))
			rows.Add(["/Subtype", FormatValue(dict.Get("Subtype")), GetFontSubtypeDesc(dict.GetName("Subtype"))]);
		if (dict.ContainsKey("BaseFont"))
			rows.Add(["/BaseFont", FormatValue(dict.Get("BaseFont")), "Font name"]);
		if (dict.ContainsKey("Encoding"))
			rows.Add(["/Encoding", FormatValue(dict.Get("Encoding")), "Character encoding"]);
		if (dict.ContainsKey("FirstChar"))
			rows.Add(["/FirstChar", FormatValue(dict.Get("FirstChar")), "First character code"]);
		if (dict.ContainsKey("LastChar"))
			rows.Add(["/LastChar", FormatValue(dict.Get("LastChar")), "Last character code"]);
		if (dict.ContainsKey("Widths"))
			rows.Add(["/Widths", FormatValue(dict.Get("Widths")), "Character widths"]);
		if (dict.ContainsKey("FontDescriptor"))
			rows.Add(["/FontDescriptor", FormatValue(dict.Get("FontDescriptor")), "Font descriptor"]);
		if (dict.ContainsKey("ToUnicode"))
			rows.Add(["/ToUnicode", FormatValue(dict.Get("ToUnicode")), "ToUnicode CMap"]);
		if (dict.ContainsKey("DescendantFonts"))
			rows.Add(["/DescendantFonts", FormatValue(dict.Get("DescendantFonts")), "CIDFont array (Type 0)"]);

		rows.AddRange(DictEntryRows(dict, "Type", "Subtype", "BaseFont", "Encoding",
			"FirstChar", "LastChar", "Widths", "FontDescriptor", "ToUnicode", "DescendantFonts"));
		return rows.ToArray();
	}

	private static string GetFontSubtypeDesc(string? subtype) => subtype switch
	{
		"Type1" => "Type 1 font",
		"TrueType" => "TrueType font",
		"Type0" => "Composite (CID) font",
		"Type3" => "Type 3 (user-defined) font",
		"CIDFontType0" => "CID Type 0 font",
		"CIDFontType2" => "CID TrueType font",
		"MMType1" => "Multiple Master Type 1",
		_ => subtype ?? "Unknown"
	};
}

/// <summary>
/// Node for a Font Descriptor (/Type /FontDescriptor)
/// </summary>
internal class FontDescriptorNode : PdfObjectNode
{
	public FontDescriptorNode(IndirectObject obj)
		: base(obj, $"FontDescriptor ({obj.ObjectNumber} 0)", "Font Descriptor", TreeViewIcon.Properties)
	{
	}

	public override object[][] GetRows()
	{
		var dict = Obj.Dictionary;
		if (dict == null) return [];

		var rows = new List<object[]>();
		if (dict.ContainsKey("FontName"))
			rows.Add(["/FontName", FormatValue(dict.Get("FontName")), "PostScript font name"]);
		if (dict.ContainsKey("Flags"))
			rows.Add(["/Flags", FormatValue(dict.Get("Flags")), "Font flags"]);
		if (dict.ContainsKey("FontBBox"))
			rows.Add(["/FontBBox", FormatValue(dict.Get("FontBBox")), "Font bounding box"]);
		if (dict.ContainsKey("ItalicAngle"))
			rows.Add(["/ItalicAngle", FormatValue(dict.Get("ItalicAngle")), "Italic angle"]);
		if (dict.ContainsKey("Ascent"))
			rows.Add(["/Ascent", FormatValue(dict.Get("Ascent")), "Ascent"]);
		if (dict.ContainsKey("Descent"))
			rows.Add(["/Descent", FormatValue(dict.Get("Descent")), "Descent"]);
		if (dict.ContainsKey("FontFile"))
			rows.Add(["/FontFile", FormatValue(dict.Get("FontFile")), "Type 1 font program"]);
		if (dict.ContainsKey("FontFile2"))
			rows.Add(["/FontFile2", FormatValue(dict.Get("FontFile2")), "TrueType font program"]);
		if (dict.ContainsKey("FontFile3"))
			rows.Add(["/FontFile3", FormatValue(dict.Get("FontFile3")), "Type 1C / CIDFontType0C / OpenType"]);

		rows.AddRange(DictEntryRows(dict, "Type", "FontName", "Flags", "FontBBox", "ItalicAngle",
			"Ascent", "Descent", "FontFile", "FontFile2", "FontFile3"));
		return rows.ToArray();
	}
}

/// <summary>
/// Node for Image XObject (/Type /XObject /Subtype /Image)
/// </summary>
internal class ImageXObjectNode : PdfObjectNode
{
	public ImageXObjectNode(IndirectObject obj)
		: base(obj, $"Image ({obj.ObjectNumber} 0)", "Image XObject", TreeViewIcon.Image)
	{
	}

	public override object[][] GetRows()
	{
		var dict = Obj.Dictionary;
		if (dict == null) return [];

		var rows = new List<object[]>();
		if (dict.ContainsKey("Width"))
			rows.Add(["/Width", FormatValue(dict.Get("Width")), "Image width in pixels"]);
		if (dict.ContainsKey("Height"))
			rows.Add(["/Height", FormatValue(dict.Get("Height")), "Image height in pixels"]);
		if (dict.ContainsKey("BitsPerComponent"))
			rows.Add(["/BitsPerComponent", FormatValue(dict.Get("BitsPerComponent")), "Bits per color component"]);
		if (dict.ContainsKey("ColorSpace"))
			rows.Add(["/ColorSpace", FormatValue(dict.Get("ColorSpace")), "Color space"]);
		if (dict.ContainsKey("Filter"))
			rows.Add(["/Filter", FormatValue(dict.Get("Filter")), "Compression filter"]);
		if (dict.ContainsKey("SMask"))
			rows.Add(["/SMask", FormatValue(dict.Get("SMask")), "Soft mask (alpha)"]);
		if (dict.ContainsKey("Interpolate"))
			rows.Add(["/Interpolate", FormatValue(dict.Get("Interpolate")), "Image interpolation"]);

		if (Obj.Value is PdfStream stream)
			rows.Add(["Stream Length", stream.DataLength, $"{stream.DataLength} bytes"]);

		rows.AddRange(DictEntryRows(dict, "Type", "Subtype", "Width", "Height",
			"BitsPerComponent", "ColorSpace", "Filter", "SMask", "Interpolate", "Length"));
		return rows.ToArray();
	}
}

/// <summary>
/// Node for Form XObject (/Type /XObject /Subtype /Form)
/// </summary>
internal class FormXObjectNode : PdfObjectNode
{
	public FormXObjectNode(IndirectObject obj)
		: base(obj, $"Form XObject ({obj.ObjectNumber} 0)", "Form XObject", TreeViewIcon.Object)
	{
	}

	public override object[][] GetRows()
	{
		var dict = Obj.Dictionary;
		if (dict == null) return [];

		var rows = new List<object[]>();
		if (dict.ContainsKey("BBox"))
			rows.Add(["/BBox", FormatValue(dict.Get("BBox")), "Form bounding box"]);
		if (dict.ContainsKey("Matrix"))
			rows.Add(["/Matrix", FormatValue(dict.Get("Matrix")), "Transformation matrix"]);
		if (dict.ContainsKey("Resources"))
			rows.Add(["/Resources", FormatValue(dict.Get("Resources")), "Form resources"]);

		if (Obj.Value is PdfStream stream)
			rows.Add(["Stream Length", stream.DataLength, $"{stream.DataLength} bytes"]);

		rows.AddRange(DictEntryRows(dict, "Type", "Subtype", "BBox", "Matrix", "Resources", "Length"));
		return rows.ToArray();
	}
}

/// <summary>
/// Node for ExtGState (/Type /ExtGState)
/// </summary>
internal class ExtGStateNode : PdfObjectNode
{
	public ExtGStateNode(IndirectObject obj)
		: base(obj, $"ExtGState ({obj.ObjectNumber} 0)", "Graphics State", TreeViewIcon.Gear)
	{
	}

	public override object[][] GetRows()
	{
		var dict = Obj.Dictionary;
		if (dict == null) return [];
		return DictEntryRows(dict, "Type");
	}
}

/// <summary>
/// Node for Annotation (/Type /Annot)
/// </summary>
internal class AnnotationNode : PdfObjectNode
{
	public AnnotationNode(IndirectObject obj)
		: base(obj, $"Annotation ({obj.ObjectNumber} 0)", "Annotation", TreeViewIcon.Comment)
	{
	}

	public override object[][] GetRows()
	{
		var dict = Obj.Dictionary;
		if (dict == null) return [];

		var rows = new List<object[]>();
		if (dict.ContainsKey("Subtype"))
			rows.Add(["/Subtype", FormatValue(dict.Get("Subtype")), "Annotation type"]);
		if (dict.ContainsKey("Rect"))
			rows.Add(["/Rect", FormatValue(dict.Get("Rect")), "Annotation rectangle"]);
		if (dict.ContainsKey("Contents"))
			rows.Add(["/Contents", FormatValue(dict.Get("Contents")), "Annotation text"]);
		if (dict.ContainsKey("A"))
			rows.Add(["/A", FormatValue(dict.Get("A")), "Action"]);
		if (dict.ContainsKey("Dest"))
			rows.Add(["/Dest", FormatValue(dict.Get("Dest")), "Destination"]);

		rows.AddRange(DictEntryRows(dict, "Type", "Subtype", "Rect", "Contents", "A", "Dest"));
		return rows.ToArray();
	}
}

/// <summary>
/// Node for Encoding (/Type /Encoding)
/// </summary>
internal class EncodingNode : PdfObjectNode
{
	public EncodingNode(IndirectObject obj)
		: base(obj, $"Encoding ({obj.ObjectNumber} 0)", "Character Encoding", TreeViewIcon.Properties)
	{
	}

	public override object[][] GetRows()
	{
		var dict = Obj.Dictionary;
		if (dict == null) return [];

		var rows = new List<object[]>();
		if (dict.ContainsKey("BaseEncoding"))
			rows.Add(["/BaseEncoding", FormatValue(dict.Get("BaseEncoding")), "Base encoding"]);
		if (dict.ContainsKey("Differences"))
		{
			var diff = dict.GetArray("Differences");
			rows.Add(["/Differences", diff != null ? $"[{diff.Count} entries]" : "[]", "Encoding differences"]);
		}

		rows.AddRange(DictEntryRows(dict, "Type", "BaseEncoding", "Differences"));
		return rows.ToArray();
	}
}

/// <summary>
/// Node for XRef stream object (/Type /XRef)
/// </summary>
internal class XRefStreamNode : PdfObjectNode
{
	public XRefStreamNode(IndirectObject obj)
		: base(obj, $"XRef Stream ({obj.ObjectNumber} 0)", "Cross-Reference Stream", TreeViewIcon.Table)
	{
	}

	public override object[][] GetRows()
	{
		var dict = Obj.Dictionary;
		if (dict == null) return [];

		var rows = new List<object[]>();
		if (dict.ContainsKey("Size"))
			rows.Add(["/Size", FormatValue(dict.Get("Size")), "Total xref entries"]);
		if (dict.ContainsKey("W"))
			rows.Add(["/W", FormatValue(dict.Get("W")), "Field widths [type, field2, field3]"]);
		if (dict.ContainsKey("Index"))
			rows.Add(["/Index", FormatValue(dict.Get("Index")), "Subsection ranges"]);
		if (dict.ContainsKey("Prev"))
			rows.Add(["/Prev", FormatValue(dict.Get("Prev")), "Previous xref offset"]);

		if (Obj.Value is PdfStream stream)
			rows.Add(["Stream Length", stream.DataLength, $"{stream.DataLength} bytes"]);

		rows.AddRange(DictEntryRows(dict, "Type", "Size", "W", "Index", "Prev", "Length", "Filter", "DecodeParms"));
		return rows.ToArray();
	}
}

/// <summary>
/// Node for Object Stream (/Type /ObjStm)
/// </summary>
internal class ObjStreamNode : PdfObjectNode
{
	public ObjStreamNode(IndirectObject obj)
		: base(obj, $"Object Stream ({obj.ObjectNumber} 0)", "Object Stream", TreeViewIcon.Binary)
	{
	}

	public override object[][] GetRows()
	{
		var dict = Obj.Dictionary;
		if (dict == null) return [];

		var rows = new List<object[]>();
		if (dict.ContainsKey("N"))
			rows.Add(["/N", FormatValue(dict.Get("N")), "Number of compressed objects"]);
		if (dict.ContainsKey("First"))
			rows.Add(["/First", FormatValue(dict.Get("First")), "Byte offset of first object"]);

		if (Obj.Value is PdfStream stream)
			rows.Add(["Stream Length", stream.DataLength, $"{stream.DataLength} bytes"]);

		rows.AddRange(DictEntryRows(dict, "Type", "N", "First", "Length", "Filter", "DecodeParms"));
		return rows.ToArray();
	}
}

/// <summary>
/// Node for Metadata stream (/Type /Metadata)
/// </summary>
internal class MetadataNode : PdfObjectNode
{
	public MetadataNode(IndirectObject obj)
		: base(obj, $"Metadata ({obj.ObjectNumber} 0)", "XMP Metadata", TreeViewIcon.Information)
	{
	}

	public override object[][] GetRows()
	{
		var dict = Obj.Dictionary;
		if (dict == null) return [];

		var rows = new List<object[]>();
		if (dict.ContainsKey("Subtype"))
			rows.Add(["/Subtype", FormatValue(dict.Get("Subtype")), "Metadata subtype"]);

		if (Obj.Value is PdfStream stream)
			rows.Add(["Stream Length", stream.DataLength, $"{stream.DataLength} bytes"]);

		rows.AddRange(DictEntryRows(dict, "Type", "Subtype", "Length"));
		return rows.ToArray();
	}
}

/// <summary>
/// Node for the Document Info dictionary (usually has /Title, /Author, etc. but no /Type)
/// </summary>
internal class InfoDictNode : PdfObjectNode
{
	public InfoDictNode(IndirectObject obj)
		: base(obj, $"Info ({obj.ObjectNumber} 0)", "Document Info", TreeViewIcon.Information)
	{
	}

	public override object[][] GetRows()
	{
		var dict = Obj.Dictionary;
		if (dict == null) return [];

		var rows = new List<object[]>();
		string[] knownKeys = ["Title", "Author", "Subject", "Keywords", "Creator", "Producer", "CreationDate", "ModDate", "Trapped"];
		foreach (var key in knownKeys)
		{
			if (dict.ContainsKey(key))
				rows.Add([$"/{key}", FormatValue(dict.Get(key)), GetInfoKeyDesc(key)]);
		}

		var handledKeys = new HashSet<string>(knownKeys);
		rows.AddRange(DictEntryRows(dict, handledKeys.ToArray()));
		return rows.ToArray();
	}

	private static string GetInfoKeyDesc(string key) => key switch
	{
		"Title" => "Document title",
		"Author" => "Document author",
		"Subject" => "Document subject",
		"Keywords" => "Keywords",
		"Creator" => "Creating application",
		"Producer" => "PDF producer",
		"CreationDate" => "Creation date",
		"ModDate" => "Modification date",
		"Trapped" => "Trapping status",
		_ => ""
	};
}

/// <summary>
/// Generic node for objects that don't have a specific node type.
/// Displays all dictionary entries.
/// </summary>
internal class GenericObjectNode : PdfObjectNode
{
	public GenericObjectNode(IndirectObject obj)
		: base(obj, $"{obj.DisplayName} ({obj.ObjectNumber} 0)", obj.DisplayName, GetObjectIcon(obj))
	{
	}

	public override object[][] GetRows()
	{
		var dict = Obj.Dictionary;
		if (dict != null)
			return DictEntryRows(dict, "Type");

		// For non-dictionary objects, show the value directly
		return [["Value", FormatValue(Obj.Value), Obj.Value.ObjectType.ToString()]];
	}

	private static TreeViewIcon GetObjectIcon(IndirectObject obj)
	{
		if (obj.Value is PdfStream) return TreeViewIcon.Binary;
		if (obj.Value is PdfDictionary) return TreeViewIcon.Object;
		if (obj.Value is PdfArray) return TreeViewIcon.Object;
		return TreeViewIcon.NullIcon;
	}
}
