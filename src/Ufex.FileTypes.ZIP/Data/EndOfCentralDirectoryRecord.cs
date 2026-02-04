using System;
using System.IO;
using Ufex.API;
using Ufex.API.Validation;

namespace Ufex.FileTypes.ZIP.Data;

internal class EndOfCentralDirectoryRecord : Section
{
	public UInt32 EndOfCentralDirSignature { get; init; }
	public UInt16 NumberOfThisDisk { get; init; }
	public UInt16 DiskWhereCentralDirectoryStarts { get; init; }
	public UInt16 NumberOfCentralDirectoryRecordsOnThisDisk { get; init; }
	public UInt16 TotalNumberOfCentralDirectoryRecords { get; init; }
	public UInt32 SizeOfCentralDirectory { get; init; }
	public UInt32 OffsetOfStartOfCentralDirectoryWithRespectToStartingDiskNumber { get; init; }
	public UInt16 ZIPFileCommentLength { get; init; }
	public byte[] ZIPFileComment { get; init; } = Array.Empty<byte>();

	public string ZIPFileCommentText { get { return DecodeString(ZIPFileComment, 0x0000); } } // TODO: GeneralPurposeBitFlag?

	public EndOfCentralDirectoryRecord(BinaryReader br)
	{
		StartRead(br);
		EndOfCentralDirSignature = br.ReadUInt32();
		NumberOfThisDisk = br.ReadUInt16();
		DiskWhereCentralDirectoryStarts = br.ReadUInt16();
		NumberOfCentralDirectoryRecordsOnThisDisk = br.ReadUInt16();
		TotalNumberOfCentralDirectoryRecords = br.ReadUInt16();
		SizeOfCentralDirectory = br.ReadUInt32();
		OffsetOfStartOfCentralDirectoryWithRespectToStartingDiskNumber = br.ReadUInt32();
		ZIPFileCommentLength = br.ReadUInt16();
		ZIPFileComment = br.ReadBytes(ZIPFileCommentLength);
		EndRead(br);
	}
}