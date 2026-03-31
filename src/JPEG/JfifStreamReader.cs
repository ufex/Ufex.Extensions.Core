using Microsoft.Extensions.Logging;
using Ufex.API;
using Ufex.API.Validation;
using Ufex.Extensions.Core.EXIF;
using Ufex.Extensions.Core.EXIF.Data;
using Ufex.Extensions.Core.JPEG.Data;

namespace Ufex.Extensions.Core.JPEG;

/// <summary>
/// Reads and parses JPEG/JFIF files by scanning marker segments sequentially.
/// JPEG files consist of a series of marker segments, each beginning with 0xFF
/// followed by a marker type byte. Most markers have a 2-byte length field
/// and a data payload.
/// </summary>
public class JfifStreamReader
{
	private readonly Stream _fileStream;

	public Logger Log { get; set; }
	public ValidationReport ValidationReport { get; set; }

	/// <summary>
	/// All parsed marker segments in file order
	/// </summary>
	internal List<Segment> Segments { get; private set; } = new();

	/// <summary>
	/// The JFIF APP0 segment (if present)
	/// </summary>
	internal App0JfifSegment? JfifApp0 { get; private set; }

	/// <summary>
	/// The SOF segment (contains image dimensions)
	/// </summary>
	internal SofSegment? Sof { get; private set; }

	/// <summary>
	/// Whether the file is a valid JFIF file
	/// </summary>
	public bool IsValid { get; private set; }
	public ExifData? ExifData { get; private set; }
	public long? ExifSegmentOffset { get; private set; }

	public JfifStreamReader(Stream fileStream, Logger log, ValidationReport validationReport)
	{
		_fileStream = fileStream;
		Log = log;
		ValidationReport = validationReport;
	}

	public bool Read()
	{
		FileReader fr = new FileReader(_fileStream, Endian.Big);

		// Validate file signature (SOI marker: FF D8)
		byte[] signature = fr.ReadBytes(2);
		if (!signature.SequenceEqual(Signatures.FileSignature))
		{
			ValidationReport.Error("Invalid JPEG file signature. Expected FF D8.");
			throw new Exception("Invalid JPEG file signature.");
		}

		// Rewind to let SOI segment read from the start
		_fileStream.Seek(0, SeekOrigin.Begin);

		while (_fileStream.Position < _fileStream.Length)
		{
			// Find next marker (skip padding/fill bytes)
			byte? markerType = FindNextMarker(fr);
			if (markerType == null)
			{
				Log.LogInformation("Reached end of file while searching for marker");
				break;
			}

			// Rewind to the start of the marker so constructors can read it
			_fileStream.Seek(-2, SeekOrigin.Current);

			Log.LogInformation($"Reading marker {Constants.GetMarkerName(markerType.Value)} (0x{markerType.Value:X2}) at position {_fileStream.Position}");

			Segment? segment = Segment.CreateSegment(markerType.Value, fr);
			if (segment == null)
			{
				// Unknown marker - read as generic segment and skip its data
				segment = SkipUnknownSegment(fr, markerType.Value);
			}

			Segments.Add(segment);
			Log.LogInformation($"Parsed {segment.MarkerName} at offset {segment.Offset}, length {segment.Length}");

			// Track important segments
			if (segment is App0JfifSegment jfif)
				JfifApp0 = jfif;
			else if (segment is SofSegment sof)
				Sof = sof;
			else if (segment is AppNSegment appn && appn.AppIdentifierString == "Exif" && ExifData == null)
				TryReadExif(segment, appn);

			// Handle SOS: skip past entropy-coded data to find the next marker
			if (segment is SosSegment sos)
			{
				SkipScanData(fr, sos);
			}

			// Stop after EOI
			if (segment is EoiSegment)
			{
				Log.LogInformation("Reached EOI marker");
				break;
			}
		}

		// Validate JFIF structure
		if (JfifApp0 == null)
		{
			ValidationReport.Warning("No JFIF APP0 marker found. File may not be a valid JFIF file.");
		}

		if (Sof == null)
		{
			ValidationReport.Warning("No SOF marker found. Could not determine image dimensions.");
		}

		IsValid = true;
		ValidationReport.Info($"Successfully parsed {Segments.Count} marker segments");
		return true;
	}

	private void TryReadExif(Segment segment, AppNSegment appn)
	{
		long appDataStart = segment.Offset + 4;
		long tiffStart = appDataStart + appn.AppIdentifier.Length;
		long exifLength = segment.Length - 2 - appn.AppIdentifier.Length;

		if (appn.AppData.Length > 0 && appn.AppData[0] == 0)
		{
			tiffStart += 1;
			exifLength -= 1;
		}

		if (exifLength <= 8)
		{
			ValidationReport.Warning("EXIF APP1 segment is too short to contain a TIFF header.");
			return;
		}

		var exifReader = new ExifStreamReader(_fileStream, Log, ValidationReport, tiffStart, exifLength);
		if (exifReader.Read())
		{
			ExifData = exifReader.ExifData;
			ExifSegmentOffset = segment.Offset;
		}
	}

	/// <summary>
	/// Finds the next marker in the stream, skipping any padding/fill bytes (0xFF).
	/// Returns the marker type byte, or null if EOF is reached.
	/// The stream is positioned after the marker type byte upon return.
	/// </summary>
	private byte? FindNextMarker(FileReader fr)
	{
		while (_fileStream.Position < _fileStream.Length)
		{
			byte b = fr.ReadByte();
			if (b != 0xFF)
				continue;

			// Read the marker type byte (skip fill/padding 0xFF bytes)
			while (_fileStream.Position < _fileStream.Length)
			{
				byte markerType = fr.ReadByte();
				if (markerType == 0xFF)
					continue; // Skip fill bytes
				if (markerType == 0x00)
					break; // Stuffed byte (escaped 0xFF in data), not a marker
				return markerType;
			}
		}

		return null;
	}

	/// <summary>
	/// Skips an unknown marker segment by reading its length and advancing past its data
	/// </summary>
	private Segment SkipUnknownSegment(FileReader fr, byte markerType)
	{
		var segment = new Segment(fr);
		ValidationReport.Warning($"Unknown marker: 0x{markerType:X2} at offset {segment.Offset}, skipping {segment.Length} bytes");

		// Segment constructor already read marker (2 bytes) + length (2 bytes)
		// Skip remaining data: Length minus the 2-byte length field
		int remainingData = segment.Length - 2;
		if (remainingData > 0 && _fileStream.Position + remainingData <= _fileStream.Length)
		{
			_fileStream.Seek(remainingData, SeekOrigin.Current);
		}

		return segment;
	}

	/// <summary>
	/// Skips the entropy-coded scan data that follows a SOS marker segment.
	/// Scans byte-by-byte looking for 0xFF followed by a non-zero, non-0xFF byte
	/// which indicates the start of the next marker.
	/// </summary>
	private void SkipScanData(FileReader fr, SosSegment sos)
	{
		long scanStart = _fileStream.Position;
		sos.ScanDataOffset = scanStart;

		while (_fileStream.Position < _fileStream.Length)
		{
			byte b = fr.ReadByte();
			if (b != 0xFF)
				continue;

			if (_fileStream.Position >= _fileStream.Length)
				break;

			byte next = fr.ReadByte();
			if (next == 0x00)
				continue; // Byte-stuffed 0xFF in scan data
			if (next == 0xFF)
			{
				// Fill byte - keep looking
				_fileStream.Seek(-1, SeekOrigin.Current);
				continue;
			}

			// Found a real marker - rewind to before the 0xFF
			_fileStream.Seek(-2, SeekOrigin.Current);
			break;
		}

		sos.ScanDataLength = _fileStream.Position - scanStart;
		Log.LogInformation($"Skipped {sos.ScanDataLength} bytes of scan data");
	}
}
