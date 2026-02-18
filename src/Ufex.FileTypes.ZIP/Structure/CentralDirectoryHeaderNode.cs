using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Format;
using Ufex.FileTypes.ZIP.Data;

namespace Ufex.FileTypes.ZIP.Structure;

internal class CentralDirectoryHeaderNode : SectionNode
{
	public CentralDirectoryHeaderNode(CentralDirectoryHeader centralDirectoryHeader)
		: base(centralDirectoryHeader, "Central Directory Header", TreeViewIcon.Header)
	{
	}

	public override string Description
	{
		get { return "Central Directory Header (" + ((CentralDirectoryHeader)Section).FileNameText + ")"; }
	}

	public override object[][] GetRows()
	{
		var d = (CentralDirectoryHeader)Section;
		object[][] rows = [
			["Central File Header Signature", d.CentralFileHeaderSignature],
			["Version Made By", d.VersionMadeBy],
			["Version Needed To Extract", d.VersionNeededToExtract],
			["General Purpose Bit Flag", d.GeneralPurposeBitFlag],
			["Compression Method", d.CompressionMethod, Constants.CompressionMethodDescription(d.CompressionMethod)],
			["Last Mod File Time", d.LastModFileTime, d.LastModFileTimeText],
			["Last Mod File Date", d.LastModFileDate, d.LastModFileDateText],
			["CRC-32", d.Crc32],
			["Compressed Size", d.CompressedSize, ByteCountFormatter.Format(d.CompressedSize)],
			["Uncompressed Size", d.UncompressedSize, ByteCountFormatter.Format(d.UncompressedSize)],
			["File Name Length", d.FileNameLength, ByteCountFormatter.Format(d.FileNameLength)],
			["Extra Field Length", d.ExtraFieldLength, ByteCountFormatter.Format(d.ExtraFieldLength)],
			["File Comment Length", d.FileCommentLength, ByteCountFormatter.Format(d.FileCommentLength)],
			["Disk Number Start", d.DiskNumberStart],
			["Internal File Attributes", d.InternalFileAttributes],
			["External File Attributes", d.ExternalFileAttributes],
			["Relative Offset of Local Header", d.RelativeOffsetOfLocalHeader],
			["File Name", d.FileName, d.FileNameText],
			["Extra Field", d.ExtraField],
			["File Comment", d.FileComment, d.FileCommentText]
		];
		return rows;
	}
}
