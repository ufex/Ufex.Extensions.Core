
using System.Linq;
using Ufex.API;
using Ufex.API.Validation;
using Ufex.Extensions.Core.EXIF.Data;

namespace Ufex.Extensions.Core.EXIF;

/// <summary>
/// Reads and parses an EXIF segment from a stream.
/// </summary>
public class ExifStreamReader
{
	private readonly Stream _fileStream;
	private long _tiffStart;
	private long _tiffEnd;

	public Logger Log { get; set; }
	public ValidationReport ValidationReport { get; set; }
	public ExifData? ExifData { get; private set; }

	public long Offset { get; private set; }
	public long Length { get; private set; }

	public ExifStreamReader(Stream fileStream, Logger log, ValidationReport validationReport, long offset, long length)
	{
		_fileStream = fileStream;
		Log = log;
		ValidationReport = validationReport;
		Offset = offset;
		Length = length;
	}

	public bool Read()
	{
		_tiffStart = Offset;
		_tiffEnd = Offset + Length;

		if (Length < 8)
		{
			ValidationReport.Error("EXIF block is too small for TIFF header.");
			return false;
		}

		FileReader fr = new FileReader(_fileStream, Endian.Big);
		fr.BaseStream.Seek(_tiffStart, SeekOrigin.Begin);

		byte[] byteOrderBytes = fr.ReadBytes(2);
		Endian byteOrder;
		if (byteOrderBytes.SequenceEqual([(byte)0x49, (byte)0x49]))
		{
			byteOrder = Endian.Little;
		}
		else if (byteOrderBytes.SequenceEqual([(byte)0x4D, (byte)0x4D]))
		{
			byteOrder = Endian.Big;
		}
		else
		{
			ValidationReport.Error("Invalid EXIF TIFF byte order marker.");
			return false;
		}

		fr.Endian = byteOrder;

		UInt16 magic = fr.ReadUInt16();
		UInt32 ifd0Offset = fr.ReadUInt32();

		TiffHeader header = new TiffHeader
		{
			ByteOrder = byteOrder,
			Magic = magic,
			Ifd0Offset = ifd0Offset,
		};

		if (!header.IsValid)
		{
			ValidationReport.Error($"Invalid EXIF TIFF magic value: 0x{magic:X4}");
			return false;
		}

		HashSet<UInt32> visited = [];

		Ifd? ifd0 = ReadIfd(fr, ifd0Offset, IfdType.IFD0, visited);
		if (ifd0 == null)
		{
			ValidationReport.Error("Failed to read EXIF IFD0.");
			return false;
		}

		Ifd? exifIfd = null;
		Ifd? gpsIfd = null;
		Ifd? ifd1 = null;

		IfdEntry? exifPointer = ifd0.FindEntry(0x8769);
		if (exifPointer != null)
		{
			UInt32 exifIfdOffset = exifPointer.GetFirstUInt32Value(byteOrder);
			if (exifIfdOffset != 0)
				exifIfd = ReadIfd(fr, exifIfdOffset, IfdType.ExifIFD, visited);
		}

		IfdEntry? gpsPointer = ifd0.FindEntry(0x8825);
		if (gpsPointer != null)
		{
			UInt32 gpsIfdOffset = gpsPointer.GetFirstUInt32Value(byteOrder);
			if (gpsIfdOffset != 0)
				gpsIfd = ReadIfd(fr, gpsIfdOffset, IfdType.GPSIFD, visited);
		}

		if (ifd0.NextIfdOffset != 0)
			ifd1 = ReadIfd(fr, ifd0.NextIfdOffset, IfdType.IFD1, visited);

		ExifData = new ExifData
		{
			TiffHeader = header,
			Ifd0 = ifd0,
			ExifIfd = exifIfd,
			GpsIfd = gpsIfd,
			Ifd1 = ifd1,
		};

		return true;
	}

	private Ifd? ReadIfd(FileReader fr, UInt32 ifdOffset, IfdType ifdType, HashSet<UInt32> visited)
	{
		if (!visited.Add(ifdOffset))
		{
			ValidationReport.Warning($"Skipping already visited {ifdType} offset 0x{ifdOffset:X8}.");
			return null;
		}

		long absoluteOffset = _tiffStart + ifdOffset;
		if (absoluteOffset < _tiffStart || absoluteOffset + 2 > _tiffEnd)
		{
			ValidationReport.Warning($"IFD offset out of range for {ifdType}: 0x{ifdOffset:X8}");
			return null;
		}

		fr.BaseStream.Seek(absoluteOffset, SeekOrigin.Begin);
		UInt16 entryCount = fr.ReadUInt16();

		Ifd ifd = new Ifd
		{
			IfdType = ifdType,
			Offset = absoluteOffset,
		};

		for (Int32 i = 0; i < entryCount; i++)
		{
			long entryOffset = fr.BaseStream.Position;
			if (entryOffset + 12 > _tiffEnd)
			{
				ValidationReport.Warning($"Truncated IFD entry in {ifdType} at offset 0x{entryOffset:X}");
				break;
			}

			byte[] entryBytes = fr.ReadBytes(12);
			UInt16 tag = ReadUInt16(entryBytes, 0, fr.Endian);
			UInt16 fieldType = ReadUInt16(entryBytes, 2, fr.Endian);
			UInt32 count = ReadUInt32(entryBytes, 4, fr.Endian);
			UInt32 valueOffset = ReadUInt32(entryBytes, 8, fr.Endian);

			Int32 typeSize = ExifConstants.GetTypeSize(fieldType);
			Int64 valueLength = (Int64)typeSize * count;
			if (valueLength < 0)
				valueLength = 0;

			IfdEntry entry = new IfdEntry
			{
				Offset = entryOffset,
				Tag = tag,
				FieldType = fieldType,
				Count = count,
				ValueOffset = valueOffset,
				InlineValue = entryBytes[8..12],
			};

			if (valueLength <= 4)
			{
				Int32 len = (Int32)valueLength;
				entry.RawValue = len > 0 ? entryBytes[8..(8 + len)] : [];
			}
			else
			{
				long valueAbsoluteOffset = _tiffStart + valueOffset;
				if (valueAbsoluteOffset >= _tiffStart && valueAbsoluteOffset < _tiffEnd)
				{
					Int64 maxReadable = _tiffEnd - valueAbsoluteOffset;
					Int32 readLen = (Int32)Math.Min(valueLength, maxReadable);
					long savedOffset = fr.BaseStream.Position;
					fr.BaseStream.Seek(valueAbsoluteOffset, SeekOrigin.Begin);
					entry.RawValue = readLen > 0 ? fr.ReadBytes(readLen) : [];
					fr.BaseStream.Seek(savedOffset, SeekOrigin.Begin);
					if (readLen < valueLength)
						ValidationReport.Warning($"Truncated EXIF value for tag 0x{tag:X4}.");
				}
				else
				{
					ValidationReport.Warning($"EXIF value offset out of range for tag 0x{tag:X4}: 0x{valueOffset:X8}");
					entry.RawValue = [];
				}
			}

			ifd.Entries.Add(entry);
		}

		if (fr.BaseStream.Position + 4 <= _tiffEnd)
			ifd.NextIfdOffset = fr.ReadUInt32();
		else
			ifd.NextIfdOffset = 0;

		return ifd;
	}

	private static UInt16 ReadUInt16(byte[] data, Int32 offset, Endian endian)
	{
		return endian == Endian.Little
			? (UInt16)(data[offset] | (data[offset + 1] << 8))
			: (UInt16)(data[offset + 1] | (data[offset] << 8));
	}

	private static UInt32 ReadUInt32(byte[] data, Int32 offset, Endian endian)
	{
		return endian == Endian.Little
			? (UInt32)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24))
			: (UInt32)(data[offset + 3] | (data[offset + 2] << 8) | (data[offset + 1] << 16) | (data[offset] << 24));
	}

}
