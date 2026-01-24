using System.Dynamic;
using System.Collections;
using System.Text;
using Ufex.API;

namespace Ufex.FileTypes.ZIP;

class ZipFileReader
{
	internal class Section
	{
		public virtual void Read(BinaryReader br)
		{

		}

		protected string DecodeString(byte[] data, UInt16 generalBitFlag)
		{
			var encoding = DataManip.GetBit(generalBitFlag, 11) ? Encoding.UTF8 : Encoding.GetEncoding(437);
			return encoding.GetString(data);
		}
	}

	internal class LocalFileHeader : Section
	{
		public UInt32 LocFileHeadSign { get; protected set; }
		public UInt16 VersionToExtract { get; protected set; }
		public UInt16 GeneralBitFlag { get; protected set; }
		public UInt16 CompMethod { get; protected set; }
		public UInt16 LastModFileTime { get; protected set; }
		public UInt16 LastModFileDate { get; protected set; }
		public UInt32 Crc32 { get; protected set; }
		public UInt32 CompSize { get; protected set; }
		public UInt32 UnCompSize { get; protected set; }
		public UInt16 FileNameLen { get; protected set; }
		public UInt16 ExtraFieldLen { get; protected set; }
		public byte[] FileName { get; protected set; } = Array.Empty<byte>();
		public byte[] ExtraField { get; protected set; } = Array.Empty<byte>();

		public bool HasDataDescriptor
		{
			get 
			{
				// Bit 3 of General Purpose Bit Flag indicates presence of Data Descriptor
				return (GeneralBitFlag & 0x08) != 0;
			}
		}

		public string FileNameText
		{
			get 
			{
				return DecodeString(FileName, GeneralBitFlag);
			}
		}

		public override void Read(BinaryReader br)
		{
			LocFileHeadSign = br.ReadUInt32();
			VersionToExtract = br.ReadUInt16();
			GeneralBitFlag = br.ReadUInt16();
			CompMethod = br.ReadUInt16();
			LastModFileTime = br.ReadUInt16();
			LastModFileDate = br.ReadUInt16();
			Crc32 = br.ReadUInt32();
			CompSize = br.ReadUInt32();
			UnCompSize = br.ReadUInt32();
			FileNameLen = br.ReadUInt16();
			ExtraFieldLen = br.ReadUInt16();
			FileName = br.ReadBytes(FileNameLen);
			ExtraField = br.ReadBytes(ExtraFieldLen);
		}
	}

	internal class FileData : Section
	{
		//Byte [] compData;
		public void Read(BinaryReader br, UInt32 size)
		{
			//problem!!!!!
			//compData = br.ReadBytes((int)size);
			br.BaseStream.Seek((long)size, SeekOrigin.Current);
		}		
	}

	internal class DataDescriptor : Section
	{
		public UInt32 Crc32 { get; protected set; }
		public UInt32 CompressedSize { get; protected set; }
		public UInt32 UncompressedSize { get; protected set; }

		public override void Read(BinaryReader br)
		{
			Crc32 = br.ReadUInt32();
			CompressedSize = br.ReadUInt32();
			UncompressedSize = br.ReadUInt32();
		}
	}

	internal class CompressedFile : Section
	{
		public LocalFileHeader Header { get; protected set; }
		public FileData FileData { get; protected set; }
		public DataDescriptor DataDescriptor { get; protected set; }

		public override void Read(BinaryReader br)
		{
			// Read Local File Header
			Header = new LocalFileHeader();
			Header.Read(br);

			// Read File Data
			FileData = new FileData();
			FileData.Read(br, Header.CompSize);

			// Read Data Descriptor
			if(Header.HasDataDescriptor)
			{
				DataDescriptor = new DataDescriptor();
				DataDescriptor.Read(br);
			}
		}
	}

	internal class CentralDirectoryHeader : Section
	{
		public UInt32 CentralFileHeaderSignature { get; protected set; }
		public UInt16 VersionMadeBy { get; protected set; }
		public UInt16 VersionNeededToExtract { get; protected set; }
		public UInt16 GeneralPurposeBitFlag { get; protected set; }
		public UInt16 CompressionMethod { get; protected set; }
		public UInt16 LastModFileTime { get; protected set; }
		public UInt16 LastModFileDate { get; protected set; }
		public UInt32 Crc32 { get; protected set; }
		public UInt32 CompressedSize { get; protected set; }
		public UInt32 UncompressedSize { get; protected set; }
		public UInt16 FileNameLength { get; protected set; }
		public UInt16 ExtraFieldLength { get; protected set; }
		public UInt16 FileCommentLength { get; protected set; }
		public UInt16 DiskNumberStart { get; protected set; }
		public UInt16 InternalFileAttributes { get; protected set; }
		public UInt32 ExternalFileAttributes { get; protected set; }
		public UInt32 RelativeOffsetOfLocalHeader { get; protected set; }
		public byte[] FileName { get; protected set; } = Array.Empty<byte>();
		public byte[] ExtraField { get; protected set; } = Array.Empty<byte>();
		public byte[] FileComment { get; protected set; } = Array.Empty<byte>();

		public string FileNameText { get { return DecodeString(FileName, GeneralPurposeBitFlag); } }
		public string FileCommentText { get { return DecodeString(FileComment, GeneralPurposeBitFlag); } }

		public override void Read(BinaryReader br)
		{
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
		}
	}

	internal class EndOfCentralDirectoryRecord : Section
	{
		public UInt32 EndOfCentralDirSignature { get; protected set; }
		public UInt16 NumberOfThisDisk { get; protected set; }
		public UInt16 DiskWhereCentralDirectoryStarts { get; protected set; }
		public UInt16 NumberOfCentralDirectoryRecordsOnThisDisk { get; protected set; }
		public UInt16 TotalNumberOfCentralDirectoryRecords { get; protected set; }
		public UInt32 SizeOfCentralDirectory { get; protected set; }
		public UInt32 OffsetOfStartOfCentralDirectoryWithRespectToStartingDiskNumber { get; protected set; }
		public UInt16 ZIPFileCommentLength { get; protected set; }
		public byte[] ZIPFileComment { get; protected set; } = Array.Empty<byte>();

		public override void Read(BinaryReader br)
		{
			EndOfCentralDirSignature = br.ReadUInt32();
			NumberOfThisDisk = br.ReadUInt16();
			DiskWhereCentralDirectoryStarts = br.ReadUInt16();
			NumberOfCentralDirectoryRecordsOnThisDisk = br.ReadUInt16();
			TotalNumberOfCentralDirectoryRecords = br.ReadUInt16();
			SizeOfCentralDirectory = br.ReadUInt32();
			OffsetOfStartOfCentralDirectoryWithRespectToStartingDiskNumber = br.ReadUInt32();
			ZIPFileCommentLength = br.ReadUInt16();
			ZIPFileComment = br.ReadBytes(ZIPFileCommentLength);
		}
	}

	private const UInt32 LOCAL_FILE_SIGNATURE = 0x04034b50;
	private const UInt32 CENTRAL_FILE_SIGNATURE = 0x02014b50;
	private const UInt32 DATA_DESCRIPTOR_SIGNATURE = 0x08074b50;
	private const UInt32 DIGITAL_SIGNATURE_SIGNATURE = 0x05054b50;
	private const UInt32 ARCHIVE_EXTRA_DATA_SIGNATURE = 0x08064b50;
	private const UInt32 ZIP64_END_OF_CENTRAL_DIR_SIGNATURE = 0x07064b50;
	private const UInt32 END_OF_CENTRAL_DIR_SIGNATURE = 0x06054b50;

	private Stream fileStream;

	public List<Section> Parts { get; protected set; }

	public ZipFileReader(Stream fileStream)
	{
		// Register the CP437 encoding provider
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		this.fileStream = fileStream;
		Parts = new List<Section>();
	}

	public bool Read()
	{
		BinaryReader br = new BinaryReader(fileStream);

		while(br.PeekChar() == 0x50)
		{
			UInt32 sign = br.ReadUInt32();
			// Rewind
			fileStream.Position -= 4;
			if(sign == LOCAL_FILE_SIGNATURE)
			{
				CompressedFile compFile = new CompressedFile();
				compFile.Read(br);
				Parts.Add(compFile);
			}
			else if(sign == CENTRAL_FILE_SIGNATURE)
			{
				CentralDirectoryHeader centralDir = new CentralDirectoryHeader();
				centralDir.Read(br);
				Parts.Add(centralDir);
			}
			else if(sign == END_OF_CENTRAL_DIR_SIGNATURE)
			{
				EndOfCentralDirectoryRecord endRecord = new EndOfCentralDirectoryRecord();
				endRecord.Read(br);
				Parts.Add(endRecord);
			}
			else
			{
				// Unknown section
				throw new Exception("Unknown ZIP section signature: " + sign.ToString("X8"));
			}
		}

		return true;
	}
	
}