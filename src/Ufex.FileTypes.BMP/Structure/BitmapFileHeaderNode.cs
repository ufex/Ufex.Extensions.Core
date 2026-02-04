using Ufex.API.Tree;
using Ufex.API.Format;
using Ufex.FileTypes.BMP.Data;

namespace Ufex.FileTypes.BMP.Structure;

/// <summary>
/// BITMAPFILEHEADER - BMP file header node
/// </summary>
internal class BitmapFileHeaderNode : HeaderNode
{
	private readonly BitmapFileHeader _header;

	public BitmapFileHeaderNode(BitmapFileHeader header)
		: base("File Header", "BITMAPFILEHEADER - 14 bytes", header.Offset, TreeViewIcon.Header)
	{
		_header = header;
	}

	protected override object[][] GetRows()
	{
		return [
			["bfType", _header.Type, _header.Type == Signatures.BMP_SIGNATURE ? "BM (valid)" : "Invalid"],
			["bfSize", _header.Size, ByteCountFormatter.Format(_header.Size)],
			["bfReserved1", _header.Reserved1, _header.Reserved1 == 0 ? "" : "Should be 0"],
			["bfReserved2", _header.Reserved2, _header.Reserved2 == 0 ? "" : "Should be 0"],
			["bfOffBits", _header.OffBits, $"Pixel data starts at offset {_header.OffBits}"],
		];
	}
}
