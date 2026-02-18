using System.Collections;
using System.Text;
using System.IO.Compression;
using Ufex.API;
using Ufex.API.Validation;
using Ufex.Extensions.Core.ZIP.Data;

namespace Ufex.Extensions.Core.ZIP;

public class ZipStreamReader
{
	static ZipStreamReader()
	{
		// Register the CP437 encoding provider once per process.
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
	}

	private Stream fileStream;

	public List<Section> Parts { get; init; }

	public Logger Log { get; set; }
	public ValidationReport ValidationReport { get; set; }

	public ZipStreamReader(Stream fileStream, Logger log, ValidationReport validationReport)
	{
		this.fileStream = fileStream;
		Parts = new List<Section>();
		Log = log;
		ValidationReport = validationReport;
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
			if(sign == Signatures.LOCAL_FILE_SIGNATURE)
			{
				UInt64 currentPos = (UInt64)fileStream.Position;
				if(!fileSizes.ContainsKey(currentPos))
				{
					throw new Exception("Could not find matching Central Directory entry for Local File Header at position: " + currentPos.ToString());
				}
				Parts.Add(new CompressedFile(br, fileSizes[currentPos], ValidationReport));
			}
			else if(sign == Signatures.CENTRAL_FILE_SIGNATURE)
			{
				Parts.Add(new CentralDirectoryHeader(br));
			}
			else if(sign == Signatures.END_OF_CENTRAL_DIR_SIGNATURE)
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
			if (BitConverter.ToUInt32(buffer, i) == Signatures.END_OF_CENTRAL_DIR_SIGNATURE)
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