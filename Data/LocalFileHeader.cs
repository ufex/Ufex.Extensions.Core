using System;
using System.IO;
using Ufex.API;

namespace Ufex.FileTypes.ZIP.Data;

internal class LocalFileHeader : Section
{
	public UInt32 LocFileHeadSign { get; init; }
	public UInt16 VersionToExtract { get; init; }
	public UInt16 GeneralPurposeBitFlag { get; init; }
	public UInt16 CompressionMethod { get; init; }
	public UInt16 LastModFileTime { get; init; }
	public UInt16 LastModFileDate { get; init; }
	public UInt32 Crc32 { get; init; }
	public UInt32 CompressedSize { get; init; }
	public UInt32 UncompressedSize { get; init; }
	public UInt16 FileNameLength { get; init; }
	public UInt16 ExtraFieldLength { get; init; }
	public byte[] FileName { get; init; } = Array.Empty<byte>();
	public byte[] ExtraField { get; init; } = Array.Empty<byte>();
	public string LastModFileTimeText { get { return DecodeTime(LastModFileTime); } }
	public string LastModFileDateText { get { return DecodeDate(LastModFileDate); } }

	public bool HasDataDescriptor
	{
		get 
		{
			// Bit 3 of General Purpose Bit Flag indicates presence of Data Descriptor
			return (GeneralPurposeBitFlag & 0x08) != 0;
		}
	}

	public string FileNameText { get { return DecodeString(FileName, GeneralPurposeBitFlag); } }
	public string FileCommentText { get { return DecodeString(ExtraField, GeneralPurposeBitFlag); } }

	public LocalFileHeader(BinaryReader br)
	{
		StartRead(br);
		LocFileHeadSign = br.ReadUInt32();
		VersionToExtract = br.ReadUInt16();
		GeneralPurposeBitFlag = br.ReadUInt16();
		CompressionMethod = br.ReadUInt16();
		LastModFileTime = br.ReadUInt16();
		LastModFileDate = br.ReadUInt16();
		Crc32 = br.ReadUInt32();
		CompressedSize = br.ReadUInt32();
		UncompressedSize = br.ReadUInt32();
		FileNameLength = br.ReadUInt16();
		ExtraFieldLength = br.ReadUInt16();
		FileName = br.ReadBytes(FileNameLength);
		ExtraField = br.ReadBytes(ExtraFieldLength);
		EndRead(br);
	}
}