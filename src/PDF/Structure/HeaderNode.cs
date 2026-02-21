using Ufex.API;
using Ufex.API.Tree;
using Ufex.Extensions.Core.PDF.Data;

namespace Ufex.Extensions.Core.PDF.Structure;

/// <summary>
/// Node for PDF header information (not an indirect object).
/// Uses a special constructor since the header is not an indirect object.
/// </summary>
internal class HeaderNode : TreeNode
{
	private readonly PdfHeader _header;

	public override Ufex.API.Visual.Visual[] Visuals
	{
		get { return [new Ufex.API.Visual.DataGridVisual(TableData(), "Header")]; }
	}

	public HeaderNode(PdfHeader header) : base("Header", TreeViewIcon.Header, TreeViewIcon.Header)
	{
		_header = header;
	}

	private Ufex.API.Tables.DynamicTableData TableData()
	{
		var td = new Ufex.API.Tables.DynamicTableData(4, "Zip.PropertyValueDescription");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");
		td.SetColumn(3, "Offset");

		td.AddRow("Signature", "%PDF-", "PDF file signature", new FileOffset(_header.Offset));
		td.AddRow("Version", _header.VersionString, $"PDF {_header.MajorVersion}.{_header.MinorVersion}", new FileOffset(_header.Offset));
		td.AddRow("Raw Header", _header.RawHeader, "", new FileOffset(_header.Offset));
		td.AddRow("Binary Marker", _header.HasBinaryMarker, _header.HasBinaryMarker ? "Binary content follows" : "No binary marker", new FileOffset(_header.Offset));
		td.AddRow("Header Length", _header.Length, $"{_header.Length} bytes", new FileOffset(_header.Offset));

		return td;
	}
}
