using System.Dynamic;
using System.Collections;
using System.Text;
using System.IO.Compression;
using Ufex.API;
using Ufex.API.Validation;
using System.Runtime.Intrinsics.Arm;

namespace Ufex.FileTypes.ZIP;

public class ZipFileReader
{
	public class Section
	{

		public static Logger Log;

		public long StartPosition { get; protected set; }
		public long EndPosition { get; protected set; }

		protected void StartRead(BinaryReader br)
		{
			StartPosition = (long)br.BaseStream.Position;
		}

		protected void EndRead(BinaryReader br)
		{
			EndPosition = (long)br.BaseStream.Position;
		}

		protected string DecodeString(byte[] data, UInt16 GeneralPurposeBitFlag)
		{
			var encoding = ByteUtil.GetBit(GeneralPurposeBitFlag, 11) ? Encoding.UTF8 : Encoding.GetEncoding(437);
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
			StartRead(br);
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
			ExtraField = br.ReadBytes(ExtraFieldLength);
			EndRead(br);
		}
	}

	internal class FileData : Section
	{
		private const int ChunkSize = 65536; // 64KB chunks
		public UInt32 Crc32 { get; init; }

		public List<DeflateStreamReader.Block> Blocks { get; init; }

		public FileData(BinaryReader br, ulong size, ValidationReport validationReport)
		{
			StartRead(br);
			Section.Log.Info($"Reading {size} bytes of compressed file data...");
			
			uint crc = 0xFFFFFFFF;
			ulong remaining = size;
			
			while(remaining > 0)
			{
				int bytesToRead = (int)Math.Min(remaining, ChunkSize);
				byte[] chunk = br.ReadBytes(bytesToRead);
				
				if (chunk.Length == 0)
					break; // End of stream reached
				
				// Update CRC32 for each byte in chunk
				for (int i = 0; i < chunk.Length; i++)
				{
					crc = Crc32Table[(crc ^ chunk[i]) & 0xFF] ^ (crc >> 8);
				}
				
				remaining -= (ulong)chunk.Length;
			}
			
			Crc32 = crc ^ 0xFFFFFFFF;
			EndRead(br);			
			
			// Reset stream position to the start of file data and read the blocks from the deflate stream
			if(size == 0)
			{
				Blocks = new List<DeflateStreamReader.Block>();
			}
			else 
			{
				br.BaseStream.Position = StartPosition;
				try 
				{
					Blocks = DeflateStreamReader.ReadBlocks(br.BaseStream);
				}
				catch(Exception ex)
				{
					validationReport.Error("Error reading Deflate blocks: " + ex.Message);
					Blocks = new List<DeflateStreamReader.Block>();
				}
				br.BaseStream.Position = EndPosition;
			}
		}

		// CRC32 lookup table (polynomial 0xEDB88320)
		private static readonly uint[] Crc32Table = GenerateCrc32Table();

		private static uint[] GenerateCrc32Table()
		{
			uint[] table = new uint[256];
			for(uint i = 0; i < 256; i++)
			{
				uint crc = i;
				for(int j = 0; j < 8; j++)
				{
					crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
				}
				table[i] = crc;
			}
			return table;
		}
	}

	internal class DataDescriptor : Section
	{
		public UInt32? DataDescriptorSignature { get; init; }
		public UInt32 Crc32 { get; init; }
		public UInt32 CompressedSize { get; init; }
		public UInt32 UncompressedSize { get; init; }

		public DataDescriptor(BinaryReader br)
		{
			StartRead(br);
			UInt32 possibleSignature = br.ReadUInt32();
			if(possibleSignature == DATA_DESCRIPTOR_SIGNATURE) 
			{
				DataDescriptorSignature = possibleSignature;
			} 
			else 
			{
				// No signature, rewind
				br.BaseStream.Seek(-4, SeekOrigin.Current);
			}
			Crc32 = br.ReadUInt32();
			CompressedSize = br.ReadUInt32();
			UncompressedSize = br.ReadUInt32();
			EndRead(br);
		}
	}

	internal class CompressedFile : Section
	{
		public LocalFileHeader Header { get; init; }
		public FileData FileData { get; init; }
		public DataDescriptor? DataDescriptor { get; init; }

		public CompressedFile(BinaryReader br, ulong dataSize, ValidationReport validationReport)
		{
			StartRead(br);
			// Read Local File Header
			Header = new LocalFileHeader(br);
			
			// Read File Data
			FileData = new FileData(br, dataSize, validationReport);

			// Read Data Descriptor if present
			if(Header.HasDataDescriptor)
			{
				DataDescriptor = new DataDescriptor(br);
			}
			EndRead(br);
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
	public ValidationReport ValidationReport { get; set; }

	public ZipFileReader(Stream fileStream, Logger log, ValidationReport validationReport)
	{
		// Register the CP437 encoding provider
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		this.fileStream = fileStream;
		Parts = new List<Section>();
		Log = log;
		ValidationReport = validationReport;
		Section.Log = log;
	}

	public bool Read()
	{
		FileReader br = new FileReader(fileStream, Endian.Little);
		Dictionary<UInt64, UInt64> fileSizes = FindSections(br);
		foreach(var kvp in fileSizes)
		{
			Log.Info($"Found Local File Header at offset {kvp.Key} with compressed size {kvp.Value}");
		}
		// Reset to start
		fileStream.Position = 0;
		while(br.PeekByte() == 0x50)
		{
			UInt32 sign = br.ReadUInt32();
			Log.Info("Reading ZIP section with signature: " + sign.ToString("X8"));
			Log.Info("  File stream position: " + fileStream.Position.ToString());

			// Rewind
			fileStream.Position -= 4;
			if(sign == LOCAL_FILE_SIGNATURE)
			{
				UInt64 currentPos = (UInt64)fileStream.Position;
				if(!fileSizes.ContainsKey(currentPos))
				{
					throw new Exception("Could not find matching Central Directory entry for Local File Header at position: " + currentPos.ToString());
				}
				Parts.Add(new CompressedFile(br, fileSizes[currentPos], ValidationReport));
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
				ValidationReport.Error("Unknown ZIP section signature " + sign.ToString("X8") + " at position " + fileStream.Position.ToString());
				throw new Exception("Unknown ZIP section signature: " + sign.ToString("X8"));
			}
			Log.Info("File stream position: " + fileStream.Position.ToString());
		}
		Log.Info("Finished reading ZIP file. Peek Byte = " + br.PeekByte().ToString());
		Log.Info("File stream position: " + fileStream.Position.ToString());
		return true;
	}

	Dictionary<UInt64, UInt64> FindSections(BinaryReader br)
	{
		Dictionary<UInt64, UInt64> locations = new Dictionary<UInt64, UInt64>();
    var (cdOffset, entryCount) = FindCentralDirectory(br);
    br.BaseStream.Position = (long)cdOffset;
		for(ulong i = 0; i < entryCount; i++)
		{
			var (offset, size) = ReadCentralDirectoryEntry(br);
			locations[offset] = size;
		}
		ValidationReport.Info($"Found {locations.Count} file entries in ZIP Central Directory");
		return locations;
	}

	(ulong cdOffset, ulong entryCount) FindCentralDirectory(BinaryReader br)
	{
		var fs = br.BaseStream;
		long fileSize = fs.Length;
		int searchSize = (int)Math.Min(fileSize, 65557);

		fs.Position = fileSize - searchSize;
		byte[] buffer = br.ReadBytes(searchSize);

		for (int i = buffer.Length - 4; i >= 0; i--)
		{
			if (BitConverter.ToUInt32(buffer, i) == END_OF_CENTRAL_DIR_SIGNATURE)
			{
				using var ms = new MemoryStream(buffer, i, buffer.Length - i);
				using var eocd = new BinaryReader(ms);

				EndOfCentralDirectoryRecord endOfCentralDirectoryRecord = new EndOfCentralDirectoryRecord(eocd);
				ulong cdOffset = endOfCentralDirectoryRecord.OffsetOfStartOfCentralDirectoryWithRespectToStartingDiskNumber;
				ulong cdSize = endOfCentralDirectoryRecord.SizeOfCentralDirectory;
				ulong entriesTotal = endOfCentralDirectoryRecord.TotalNumberOfCentralDirectoryRecords;

				// ZIP64?
				if(entriesTotal == 0xFFFF || cdSize == 0xFFFFFFFF || cdOffset == 0xFFFFFFFF)
				{
					return ReadZip64CentralDirectory(br, fileSize);
				}

				return (cdOffset, entriesTotal);
			}
		}
		ValidationReport.Error("End of Central Directory (EOCD) record not found in ZIP file.");
		throw new InvalidDataException("EOCD not found");
	}

	(ulong cdOffset, ulong entryCount) ReadZip64CentralDirectory(BinaryReader br, long fileSize)
	{
		var fs = br.BaseStream;
		fs.Position = fileSize - 22 - 20; // EOCD + locator min

		Zip64EndOfCentralDirectoryRecord centralDirectoryRecord = new Zip64EndOfCentralDirectoryRecord(br);
		return (centralDirectoryRecord.OffsetOfStartOfCentralDirectoryWithRespectToStartingDiskNumber,
				centralDirectoryRecord.TotalNumberOfCentralDirectoryRecords);
	}

	(ulong offset, ulong size) ReadCentralDirectoryEntry(BinaryReader br)
	{
		CentralDirectoryHeader cdHeader = new CentralDirectoryHeader(br);
		ulong compSize = cdHeader.CompressedSize;
		ulong localOffset = cdHeader.RelativeOffsetOfLocalHeader;
		if(cdHeader.Zip64CompressedSize.HasValue)
		{
			compSize = cdHeader.Zip64CompressedSize.Value;
		}

		if(cdHeader.Zip64RelativeOffsetOfLocalHeader.HasValue)
		{
			localOffset = cdHeader.Zip64RelativeOffsetOfLocalHeader.Value;
		}

		return (localOffset, compSize);
	}

}