using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Format;
using Ufex.Extensions.Core.ZIP.Data;

namespace Ufex.Extensions.Core.ZIP.Structure;

internal class LocalFileHeaderNode : SectionNode
{
	public LocalFileHeaderNode(LocalFileHeader localFileHeader) 
		: base(localFileHeader, "Local File Header", TreeViewIcon.Header)
	{
	}

	public override string Description
	{
		get { return "Local File Header (" + ((LocalFileHeader)Section).FileNameText + ")"; }
	}

	public override object[][] GetRows()
	{
		var d = (LocalFileHeader)Section;
		object[][] rows = [
			["Local Header Signature", d.LocFileHeadSign],
			["Version Needed To Extract", d.VersionToExtract],
			["General Purpose Bit Flag", d.GeneralPurposeBitFlag, Constants.GeneralPurposeBitFlagDescription(d.GeneralPurposeBitFlag, d.CompressionMethod)],
			["Compression Method", d.CompressionMethod, Constants.COMPRESSION_METHODS.ContainsKey(d.CompressionMethod) ? Constants.COMPRESSION_METHODS[d.CompressionMethod] : "Unknown"],
			["Last Mod File Time", d.LastModFileTime, d.LastModFileTimeText],
			["Last Mod File Date", d.LastModFileDate, d.LastModFileDateText],
			["CRC-32", d.Crc32],
			["Compressed Size", d.CompressedSize, ByteCountFormatter.Format(d.CompressedSize)],
			["Uncompressed Size", d.UncompressedSize, ByteCountFormatter.Format(d.UncompressedSize)],
			["File Name Length", d.FileNameLength, ByteCountFormatter.Format(d.FileNameLength)],
			["Extra Field Length", d.ExtraFieldLength, ByteCountFormatter.Format(d.ExtraFieldLength)],
			["File Name", d.FileName, d.FileNameText],
			["Extra Field", d.ExtraField]
		];
		return rows;
	}
}
