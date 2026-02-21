using System;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.API.Format;
using Ufex.Extensions.Core.PDF.Data;

namespace Ufex.Extensions.Core.PDF.Structure;

/// <summary>
/// Base tree node for PDF objects displayed in the Structure tab.
/// Follows the same pattern as ChunkNode in the PNG plugin.
/// </summary>
internal class PdfObjectNode : TreeNode
{
	protected IndirectObject Obj { get; }

	public string Description { get; protected set; }

	public override Visual[] Visuals
	{
		get { return [new DataGridVisual(TableData(), "Data")]; }
	}

	public PdfObjectNode(IndirectObject obj, string text, string description, TreeViewIcon icon)
		: base(text, icon, icon)
	{
		Obj = obj;
		Description = description;
	}

	/// <summary>
	/// Factory method to create the appropriate node type for a given indirect object.
	/// </summary>
	public static PdfObjectNode FromObject(IndirectObject obj)
	{
		return obj.TypeName switch
		{
			"Catalog" => new CatalogNode(obj),
			"Pages" => new PagesNode(obj),
			"Page" => new PageNode(obj),
			"Font" => new FontNode(obj),
			"FontDescriptor" => new FontDescriptorNode(obj),
			"XObject" => CreateXObjectNode(obj),
			"ExtGState" => new ExtGStateNode(obj),
			"Annot" => new AnnotationNode(obj),
			"XRef" => new XRefStreamNode(obj),
			"ObjStm" => new ObjStreamNode(obj),
			"Encoding" => new EncodingNode(obj),
			"Metadata" => new MetadataNode(obj),
			_ => CreateGenericNode(obj)
		};
	}

	private static PdfObjectNode CreateXObjectNode(IndirectObject obj)
	{
		return obj.SubtypeName switch
		{
			"Image" => new ImageXObjectNode(obj),
			"Form" => new FormXObjectNode(obj),
			_ => new GenericObjectNode(obj)
		};
	}

	private static PdfObjectNode CreateGenericNode(IndirectObject obj)
	{
		// Check for info dictionary pattern (no /Type but has common info keys)
		if (obj.Dictionary is PdfDictionary dict)
		{
			if (dict.ContainsKey("Title") || dict.ContainsKey("Author") || dict.ContainsKey("Producer"))
				return new InfoDictNode(obj);
		}

		return new GenericObjectNode(obj);
	}

	/// <summary>
	/// Builds the DynamicTableData for this node's detail panel.
	/// </summary>
	public virtual DynamicTableData TableData()
	{
		var td = new DynamicTableData(4, "Zip.PropertyValueDescription");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");
		td.SetColumn(3, "Offset");

		// Standard header rows for every indirect object
		td.AddRow("Object Number", Obj.ObjectNumber, "", new FileOffset(Obj.Offset));
		td.AddRow("Generation", Obj.Generation, "", new FileOffset(Obj.Offset));
		td.AddRow("Object Type", Obj.Value.ObjectType.ToString(), Obj.DisplayName, new FileOffset(Obj.Offset));

		// Add subclass-specific rows
		var rows = GetRows();
		for (int i = 0; i < rows.Length; i++)
		{
			if (rows[i].Length >= 3)
				td.AddRow(rows[i][0], rows[i][1], rows[i][2], new FileOffset(Obj.Offset));
			else if (rows[i].Length >= 2)
				td.AddRow(rows[i][0], rows[i][1], "", new FileOffset(Obj.Offset));
		}

		return td;
	}

	/// <summary>
	/// Returns property rows: [property, value, description?] for the detail panel.
	/// Override in subclasses.
	/// </summary>
	public virtual object[][] GetRows()
	{
		return [];
	}

	/// <summary>
	/// Helper to get a displayable value from a PdfObject.
	/// </summary>
	internal static string FormatValue(PdfObject? obj)
	{
		if (obj == null) return "";
		return obj switch
		{
			PdfName n => n.Value,
			PdfString s => s.TextValue,
			PdfHexString h => h.ToString(),
			PdfInteger i => i.Value.ToString(),
			PdfReal r => r.Value.ToString("G"),
			PdfBoolean b => b.Value.ToString(),
			PdfNull => "null",
			PdfReference re => $"{re.ObjectNumber} {re.Generation} R",
			PdfArray a => $"[{a.Count} items]",
			PdfDictionary d => $"<<{d.Entries.Count} entries>>",
			PdfStream s2 => $"stream ({s2.DataLength} bytes)",
			_ => obj.ToString() ?? ""
		};
	}

	/// <summary>
	/// Helper to generate rows from all dictionary entries.
	/// </summary>
	protected object[][] DictEntryRows(PdfDictionary dict, params string[] excludeKeys)
	{
		var exclude = new HashSet<string>(excludeKeys);
		var rows = new List<object[]>();
		foreach (var kvp in dict.Entries)
		{
			if (exclude.Contains(kvp.Key)) continue;
			rows.Add([$"/{kvp.Key}", FormatValue(kvp.Value), kvp.Value.ObjectType.ToString()]);
		}
		return rows.ToArray();
	}
}
