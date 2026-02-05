using Ufex.API.Tree;
using Ufex.FileTypes.GIF.Data;

namespace Ufex.FileTypes.GIF.Structure;

/// <summary>
/// Header node for the GIF file header
/// </summary>
internal class HeaderNode : BlockNode
{
	private readonly Header _header;

	public HeaderNode(Header header)
		: base("Header", TreeViewIcon.Header, header.Offset)
	{
		_header = header;
		Description = $"GIF{_header.Version} Header";
	}

	protected override List<object[]> GetRows()
	{
		return new List<object[]>
		{
			new object[] { "Signature", _header.Signature, "File signature (GIF)" },
			new object[] { "Version", _header.Version, _header.IsGif89a ? "GIF89a format" : "GIF87a format" }
		};
	}
}
