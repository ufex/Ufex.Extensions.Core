using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Format;
using Ufex.Extensions.Core.ZIP.Data;

namespace Ufex.Extensions.Core.ZIP.Structure;

internal class EndOfCentralDirectoryRecordNode : SectionNode
{
	public EndOfCentralDirectoryRecordNode(EndOfCentralDirectoryRecord record)
		: base(record, "End of Central Directory Record", TreeViewIcon.Header)
	{
	}

	public override string Description
	{
		get { return "End of Central Directory Record"; }
	}

	public override object[][] GetRows()
	{
		var d = (EndOfCentralDirectoryRecord)Section;
		object[][] rows = [
			["End of Central Directory Signature", d.EndOfCentralDirSignature],
			["Number of This Disk", d.NumberOfThisDisk],
			["Disk Where Central Directory Starts", d.DiskWhereCentralDirectoryStarts],
			["Number of Central Directory Records on This Disk", d.NumberOfCentralDirectoryRecordsOnThisDisk],
			["Total Number of Central Directory Records", d.TotalNumberOfCentralDirectoryRecords],
			["Size of Central Directory", d.SizeOfCentralDirectory, ByteCountFormatter.Format(d.SizeOfCentralDirectory)],
			["Offset of Start of Central Directory", d.OffsetOfStartOfCentralDirectoryWithRespectToStartingDiskNumber],
			["ZIP File Comment Length", d.ZIPFileCommentLength, ByteCountFormatter.Format(d.ZIPFileCommentLength)],
			["ZIP File Comment", d.ZIPFileComment, d.ZIPFileCommentText]
		];
		return rows;
	}
}
