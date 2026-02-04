using Ufex.API.Tree;
using Ufex.FileTypes.BMP.Data;

namespace Ufex.FileTypes.BMP.Structure;

/// <summary>
/// ICONDIR - Icon directory header node
/// </summary>
internal class IconDirNode : HeaderNode
{
	private readonly IconDir _header;

	public IconDirNode(IconDir header)
		: base("Icon Directory", "ICONDIR - 6 bytes", header.Offset, TreeViewIcon.Header)
	{
		_header = header;
	}

	protected override object[][] GetRows()
	{
		string typeDesc = _header.Type switch
		{
			Signatures.ICO_TYPE => "Icon (.ico)",
			Signatures.CUR_TYPE => "Cursor (.cur)",
			_ => "Unknown"
		};

		return [
			["idReserved", _header.Reserved, _header.Reserved == 0 ? "" : "Should be 0"],
			["idType", _header.Type, typeDesc],
			["idCount", _header.Count, $"{_header.Count} images"],
		];
	}
}
