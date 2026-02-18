using System;
using System.IO;
using Ufex.API;
using Ufex.API.Validation;

namespace Ufex.Extensions.Core.ZIP.Data;

internal class Zip64EndOfCentralDirectoryRecord : Section
{
	public UInt32 EndOfCentralDirSignature { get; init; }
	public UInt64 SizeOfRecord { get; init; }
	public UInt16 VersionMadeBy { get; init; }
	public UInt16 VersionNeededToExtract { get; init; }
	public UInt32 NumberOfThisDisk { get; init; }
	public UInt32 NumberOfThisDiskWithStartOfCentralDirectory { get; init; }
	public UInt64 NumberOfCentralDirectoryRecordsOnThisDisk { get; init; }
	public UInt64 TotalNumberOfCentralDirectoryRecords { get; init; }
	public UInt64 SizeOfCentralDirectory { get; init; }
	public UInt64 OffsetOfStartOfCentralDirectoryWithRespectToStartingDiskNumber { get; init; }
	public byte[] ExtensibleDataSector { get; init; } = Array.Empty<byte>();

	public Zip64EndOfCentralDirectoryRecord(BinaryReader br)
	{
		StartRead(br);
		EndOfCentralDirSignature = br.ReadUInt32();
		SizeOfRecord = br.ReadUInt64();
		VersionMadeBy = br.ReadUInt16();
		VersionNeededToExtract = br.ReadUInt16();
		NumberOfThisDisk = br.ReadUInt32();
		NumberOfThisDiskWithStartOfCentralDirectory = br.ReadUInt32();
		NumberOfCentralDirectoryRecordsOnThisDisk = br.ReadUInt64();
		TotalNumberOfCentralDirectoryRecords = br.ReadUInt64();
		SizeOfCentralDirectory = br.ReadUInt64();
		OffsetOfStartOfCentralDirectoryWithRespectToStartingDiskNumber = br.ReadUInt64();
		long remainingBytes = (long)SizeOfRecord - 44; // 44 bytes read so far
		if(remainingBytes > 0)
		{
			ExtensibleDataSector = br.ReadBytes((int)remainingBytes);
		}
		EndRead(br);
	}
}

