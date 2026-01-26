using System.Dynamic;
using System.Collections;
using System.Text;
using Ufex.API;

namespace Ufex.FileTypes.ZIP;

class ZipFileReader
{
	internal class Section
	{
		protected string DecodeString(byte[] data, UInt16 GeneralPurposeBitFlag)
		{
			var encoding = DataManip.GetBit(GeneralPurposeBitFlag, 11) ? Encoding.UTF8 : Encoding.GetEncoding(437);
			return encoding.GetString(data);
		}

		/// <summary>
		/// Decodes a DOS date format into a string representation YYYY-MM-DD
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		protected string DecodeDate(UInt16 date)
		{
			int year = ((date >> 9) & 0x7F) + 1980;
			int month = (date >> 5) & 0x0F;
			int day = date & 0x1F;
			return $"{year:D4}-{month:D2}-{day:D2}";
		}

		/// <summary>
		/// Decodes a DOS time format into a string representation HH:MM:SS
		/// </summary>
		/// <param name="time"></param>
		/// <returns>Time in HH:MM:SS format</returns>
		protected string DecodeTime(UInt16 time)
		{
			int hours = (time >> 11) & 0x1F;
			int minutes = (time >> 5) & 0x3F;
			int seconds = (time & 0x1F) * 2;
			return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
		}
	}

	internal class LocalFileHeader : Section
	{
		public UInt32 LocFileHeadSign { get; init; }
		public UInt16 VersionToExtract { get; init; }
		public UInt16 GeneralPurposeBitFlag { get; init; }
		public UInt16 CompressionMethod { get; init; }
		public UInt16 LastModFileTime { get; init; }
		public UInt16 LastModFileDate { get; init; }
		public UInt32 Crc32 { get; init; }
		public UInt32 CompSize { get; init; }
		public UInt32 UnCompSize { get; init; }
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
			LocFileHeadSign = br.ReadUInt32();
			VersionToExtract = br.ReadUInt16();
			GeneralPurposeBitFlag = br.ReadUInt16();
			CompressionMethod = br.ReadUInt16();
			LastModFileTime = br.ReadUInt16();
			LastModFileDate = br.ReadUInt16();
			Crc32 = br.ReadUInt32();
			CompSize = br.ReadUInt32();
			UnCompSize = br.ReadUInt32();
			FileNameLength = br.ReadUInt16();
			ExtraFieldLength = br.ReadUInt16();
			FileName = br.ReadBytes(FileNameLength);
			ExtraField = br.ReadBytes(ExtraFieldLen);
		}
	}

	internal class FileData : Section
	{
		public FileData(BinaryReader br, UInt32 size)
		{
			br.BaseStream.Seek((long)size, SeekOrigin.Current);
		}
	}

	internal class DataDescriptor : Section
	{
		public UInt32 Crc32 { get; init; }
		public UInt32 CompressedSize { get; init; }
		public UInt32 UncompressedSize { get; init; }

		public DataDescriptor(BinaryReader br)
		{
			Crc32 = br.ReadUInt32();
			CompressedSize = br.ReadUInt32();
			UncompressedSize = br.ReadUInt32();
		}
	}

	internal class CompressedFile : Section
	{
		public LocalFileHeader Header { get; init; }
		public FileData FileData { get; init; }
		public DataDescriptor? DataDescriptor { get; init; }

		public CompressedFile(BinaryReader br)
		{
			// Read Local File Header
			Header = new LocalFileHeader(br);

			// Read File Data
			FileData = new FileData(br, Header.CompSize);

			// Read Data Descriptor
			if(Header.HasDataDescriptor)
			{
				DataDescriptor = new DataDescriptor(br);
			}
		}
	}

	internal class CentralDirectoryHeader : Section
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
		public UInt16 FileNameLengthgth { get; init; }
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

		public CentralDirectoryHeader(BinaryReader br)
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
		public UInt32 EndOfCentralDirSignature { get; init; }
		public UInt16 NumberOfThisDisk { get; init; }
		public UInt16 DiskWhereCentralDirectoryStarts { get; init; }
		public UInt16 NumberOfCentralDirectoryRecordsOnThisDisk { get; init; }
		public UInt16 TotalNumberOfCentralDirectoryRecords { get; init; }
		public UInt32 SizeOfCentralDirectory { get; init; }
		public UInt32 OffsetOfStartOfCentralDirectoryWithRespectToStartingDiskNumber { get; init; }
		public UInt16 ZIPFileCommentLength { get; init; }
		public byte[] ZIPFileComment { get; init; } = Array.Empty<byte>();

		public EndOfCentralDirectoryRecord(BinaryReader br)
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

	public List<Section> Parts { get; init; }

	public Logger Log { get; set; }

	public ZipFileReader(Stream fileStream, Logger log)
	{
		// Register the CP437 encoding provider
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		this.fileStream = fileStream;
		Parts = new List<Section>();
		Log = log;
	}

	public bool Read()
	{
		BinaryReader br = new BinaryReader(fileStream);
		int nextChar;
		while((nextChar = br.PeekChar()) == 0x50)
		{
			UInt32 sign = br.ReadUInt32();
			Log.Info("Reading ZIP section with signature: " + sign.ToString("X8"));
			
			// Rewind
			fileStream.Position -= 4;
			if(sign == LOCAL_FILE_SIGNATURE)
			{
				Parts.Add(new CompressedFile(br));
			}
			else if(sign == CENTRAL_FILE_SIGNATURE)
			{
				Parts.Add(new CentralDirectoryHeader(br));
			}
			else if(sign == END_OF_CENTRAL_DIR_SIGNATURE)
			{
				Parts.Add(new EndOfCentralDirectoryRecord(br));
			}
			else
			{
				// Unknown section
				throw new Exception("Unknown ZIP section signature: " + sign.ToString("X8"));
			}
		}
		Log.Info("Finished reading ZIP file. Peek Char = " + nextChar.ToString());
		Log.Info("File stream position: " + fileStream.Position.ToString());
		return true;
	}
	
}