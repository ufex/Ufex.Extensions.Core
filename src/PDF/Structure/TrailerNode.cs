using Ufex.API;
using Ufex.API.Tree;
using Ufex.Extensions.Core.PDF.Data;

namespace Ufex.Extensions.Core.PDF.Structure;

/// <summary>
/// Node for the trailer section.
/// </summary>
internal class TrailerNode : TreeNode
{
	private readonly PdfTrailer _trailer;

	public override Ufex.API.Visual.Visual[] Visuals
	{
		get { return [new Ufex.API.Visual.DataGridVisual(TableData(), "Trailer")]; }
	}

	public TrailerNode(PdfTrailer trailer) : base("Trailer", TreeViewIcon.Section, TreeViewIcon.Section)
	{
		_trailer = trailer;
	}

	private Ufex.API.Tables.DynamicTableData TableData()
	{
		var td = new Ufex.API.Tables.DynamicTableData(4, "Zip.PropertyValueDescription");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");
		td.SetColumn(3, "Offset");

		td.AddRow("startxref", _trailer.StartXRefOffset, "Byte offset of xref section", new FileOffset(_trailer.Offset));
		td.AddRow("XRef Type", _trailer.UsesXRefStream ? "XRef Stream" : "XRef Table", "", new FileOffset(_trailer.Offset));
		td.AddRow("/Size", _trailer.Size, "Total number of xref entries", new FileOffset(_trailer.Offset));

		if (_trailer.Root != null)
			td.AddRow("/Root", $"{_trailer.Root.ObjectNumber} {_trailer.Root.Generation} R", "Document catalog", new FileOffset(_trailer.Offset));

		if (_trailer.Info != null)
			td.AddRow("/Info", $"{_trailer.Info.ObjectNumber} {_trailer.Info.Generation} R", "Document info dictionary", new FileOffset(_trailer.Offset));

		if (_trailer.PrevOffset.HasValue)
			td.AddRow("/Prev", _trailer.PrevOffset.Value, "Previous xref section offset", new FileOffset(_trailer.Offset));

		td.AddRow("Encrypted", _trailer.IsEncrypted, _trailer.IsEncrypted ? "Document is encrypted" : "Not encrypted", new FileOffset(_trailer.Offset));

		// Show remaining trailer dict entries
		foreach (var kvp in _trailer.Dictionary.Entries)
		{
			if (kvp.Key is "Size" or "Root" or "Info" or "Prev" or "Encrypt")
				continue;
			td.AddRow($"/{kvp.Key}", PdfObjectNode.FormatValue(kvp.Value), kvp.Value.ObjectType.ToString(), new FileOffset(_trailer.Offset));
		}

		return td;
	}
}
