using System;
using System.IO;
using Ufex.API;
using Ufex.API.Validation;

namespace Ufex.Extensions.Core.ZIP.Data;

public class CentralDirectoryHeader : Section
{
	public UInt32 CentralFileHeaderSignature { get; init; }
	public UInt16 VersionMadeBy { get; init; }
	public UInt16 VersionNeededToExtract { get; init; }
	public UInt16 GeneralPurposeBitFlag { get; init; }
	public UInt16 CompressionMethod { get; init; }
	public UInt16 LastModFileTime { get; init; }
	public UInt16 LastModFileDate { get; init; }
	public UInt32 Crc32 { get; init; }
	public UInt32 CompressedSize { get; init; }
	public UInt32 UncompressedSize { get; init; }
	public UInt16 FileNameLength { get; init; }
	public UInt16 ExtraFieldLength { get; init; }
	public UInt16 FileCommentLength { get; init; }
	public UInt16 DiskNumberStart { get; init; }
	public UInt16 InternalFileAttributes { get; init; }
	public UInt32 ExternalFileAttributes { get; init; }
	public UInt32 RelativeOffsetOfLocalHeader { get; init; }
	public byte[] FileName { get; init; } = Array.Empty<byte>();
	public byte[] ExtraField { get; init; } = Array.Empty<byte>();
	public byte[] FileComment { get; init; } = Array.Empty<byte>();

	public string LastModFileTimeText { get { return DecodeTime(LastModFileTime); } }
	public string LastModFileDateText { get { return DecodeDate(LastModFileDate); } }
	public string FileNameText { get { return DecodeString(FileName, GeneralPurposeBitFlag); } }
	public string FileCommentText { get { return DecodeString(FileComment, GeneralPurposeBitFlag); } }

	public UInt64? Zip64CompressedSize { get; protected set; }
	public UInt64? Zip64UncompressedSize { get; protected set; }
	public UInt64? Zip64RelativeOffsetOfLocalHeader { get; protected set; }

	public CentralDirectoryHeader(BinaryReader br)
	{
		StartRead(br);
		CentralFileHeaderSignature = br.ReadUInt32();
		VersionMadeBy = br.ReadUInt16();
		VersionNeededToExtract = br.ReadUInt16();
		GeneralPurposeBitFlag = br.ReadUInt16();
		CompressionMethod = br.ReadUInt16();
		LastModFileTime = br.ReadUInt16();
		LastModFileDate = br.ReadUInt16();
		Crc32 = br.ReadUInt32();
		CompressedSize = br.ReadUInt32();
		UncompressedSize = br.ReadUInt32();
		FileNameLength = br.ReadUInt16();
		ExtraFieldLength = br.ReadUInt16();
		FileCommentLength = br.ReadUInt16();
		DiskNumberStart = br.ReadUInt16();
		InternalFileAttributes = br.ReadUInt16();
		ExternalFileAttributes = br.ReadUInt32();
		RelativeOffsetOfLocalHeader = br.ReadUInt32();
		FileName = br.ReadBytes(FileNameLength);
		ExtraField = br.ReadBytes(ExtraFieldLength);
		FileComment = br.ReadBytes(FileCommentLength);

		// Check for ZIP64 Extended Information Extra Field
		if (CompressedSize == 0xFFFFFFFF || UncompressedSize == 0xFFFFFFFF || RelativeOffsetOfLocalHeader == 0xFFFFFFFF)
		{
			ParseExtraField(ExtraField, ExtraFieldLength);
		}
		EndRead(br);
	}

	protected void ParseExtraField(byte[] extraField, UInt16 extraFieldLength)
	{
		using (MemoryStream ms = new MemoryStream(extraField, 0, extraFieldLength))
		using (BinaryReader br = new BinaryReader(ms))
		{
			while(ms.Position < ms.Length)
			{
				UInt16 headerId = br.ReadUInt16();
				UInt16 dataSize = br.ReadUInt16();
				if(headerId == 0x0001) // ZIP64 Extended Information Extra Field
				{
					if(UncompressedSize == 0xFFFFFFFF)
					{
						Zip64UncompressedSize = br.ReadUInt64();
					}
					if(CompressedSize == 0xFFFFFFFF)
					{
						Zip64CompressedSize = br.ReadUInt64();
					}
					if(RelativeOffsetOfLocalHeader == 0xFFFFFFFF)
					{
						Zip64RelativeOffsetOfLocalHeader = br.ReadUInt64();
					}
					// Skip the rest if any
					long bytesRead = (ms.Position - (ms.Length - dataSize));
					if (bytesRead < dataSize)
					{
						br.ReadBytes((int)(dataSize - bytesRead));
					}
				}
				else
				{
					// Skip unknown extra field
					br.ReadBytes(dataSize);
				}
			}
		}
	}
}
