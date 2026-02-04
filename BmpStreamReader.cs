using Ufex.API;
using Ufex.API.Validation;
using Ufex.FileTypes.BMP.Data;

namespace Ufex.FileTypes.BMP;

/// <summary>
/// Reads and parses Windows Bitmap (BMP) files
/// </summary>
public class BmpStreamReader
{
	private readonly Stream _fileStream;

	public Logger Log { get; set; }
	public ValidationReport ValidationReport { get; set; }

	/// <summary>
	/// The 14-byte file header
	/// </summary>
	internal BitmapFileHeader? FileHeader { get; private set; }

	/// <summary>
	/// The info header (v3, v4, or v5)
	/// </summary>
	internal BitmapInfoHeader? InfoHeader { get; private set; }

	/// <summary>
	/// The color table (if present)
	/// </summary>
	internal ColorTable? ColorTable { get; private set; }

	/// <summary>
	/// Offset where pixel data begins
	/// </summary>
	public long PixelDataOffset { get; private set; }

	/// <summary>
	/// Size of pixel data in bytes
	/// </summary>
	public long PixelDataSize { get; private set; }

	public BmpStreamReader(Stream fileStream, Logger log, ValidationReport validationReport)
	{
		_fileStream = fileStream;
		Log = log;
		ValidationReport = validationReport;
	}

	public bool Read()
	{
		FileReader fr = new FileReader(_fileStream, Endian.Little);

		// Read File Header (14 bytes)
		FileHeader = new BitmapFileHeader(fr);
		Log.Info($"Read BitmapFileHeader at offset {FileHeader.Offset}");

		// Validate signature
		if (FileHeader.Type != Signatures.BMP_SIGNATURE)
		{
			ValidationReport.Error($"Invalid BMP signature: 0x{FileHeader.Type:X4} (expected 0x4D42 'BM')");
			throw new Exception("Invalid BMP file signature.");
		}

		// Validate file size
		if (FileHeader.Size != _fileStream.Length)
		{
			ValidationReport.Warning($"bfSize ({FileHeader.Size}) does not match actual file size ({_fileStream.Length})");
		}

		// Validate reserved fields
		if (FileHeader.Reserved1 != 0)
			ValidationReport.Warning("bfReserved1 must be zero");
		if (FileHeader.Reserved2 != 0)
			ValidationReport.Warning("bfReserved2 must be zero");

		// Read Info Header - peek at size to determine version
		uint infoHeaderSize = fr.ReadUInt32();
		_fileStream.Seek(-4, SeekOrigin.Current);

		Log.Info($"Info header size: {infoHeaderSize} bytes");

		InfoHeader = infoHeaderSize switch
		{
			Constants.INFO_HEADER_V3_SIZE => new BitmapInfoHeader(fr),
			Constants.INFO_HEADER_V4_SIZE => new BitmapV4Header(fr),
			Constants.INFO_HEADER_V5_SIZE => new BitmapV5Header(fr),
			_ => throw new Exception($"Unsupported bitmap header size: {infoHeaderSize}")
		};

		Log.Info($"Read {InfoHeader.GetType().Name} at offset {InfoHeader.Offset}");

		// Validate info header
		ValidateInfoHeader();

		// Calculate number of colors in color table
		uint numColors = CalculateColorCount();
		Log.Info($"Color table size: {numColors} entries");

		// Read color table if present
		if (numColors > 0)
		{
			long expectedPosition = _fileStream.Position;
			long expectedPaletteEnd = FileHeader.OffBits;
			long expectedPaletteSize = numColors * 4;

			if (expectedPosition + expectedPaletteSize > expectedPaletteEnd)
			{
				ValidationReport.Warning($"Color table extends beyond pixel data start");
			}

			ColorTable = new ColorTable(fr, numColors);
			Log.Info($"Read ColorTable with {ColorTable.Count} entries at offset {ColorTable.Offset}");

			if (ColorTable.InvalidReservedCount > 0)
			{
				ValidationReport.Warning($"{ColorTable.InvalidReservedCount} color table entries have non-zero reserved bytes");
			}
		}

		// Calculate pixel data location
		PixelDataOffset = FileHeader.OffBits;
		PixelDataSize = _fileStream.Length - PixelDataOffset;

		ValidationReport.Info($"Successfully parsed BMP: {InfoHeader.Width}x{Math.Abs(InfoHeader.Height)}, {InfoHeader.BitsPerPixel}-bit");

		return true;
	}

	private void ValidateInfoHeader()
	{
		if (InfoHeader == null) return;

		if (InfoHeader.Planes != 1)
			ValidationReport.Warning("biPlanes must be 1");

		if (InfoHeader.Width < 0)
			ValidationReport.Warning("biWidth should be a positive number");

		if (InfoHeader.Height < 0)
			ValidationReport.Info("biHeight is negative - bitmap will be displayed as a top-down DIB");

		if (!Constants.COMPRESSION_METHODS.ContainsKey(InfoHeader.Compression))
			ValidationReport.Warning($"Unknown compression method: {InfoHeader.Compression}");
	}

	private uint CalculateColorCount()
	{
		if (InfoHeader == null) return 0;

		ushort bitsPerPixel = InfoHeader.BitsPerPixel;
		uint colorsUsed = InfoHeader.ColorsUsed;
		uint compression = InfoHeader.Compression;

		if (bitsPerPixel == 1)
		{
			return colorsUsed > 0 ? colorsUsed : 2;
		}
		else if (bitsPerPixel > 1 && bitsPerPixel < 16)
		{
			return colorsUsed > 0 ? colorsUsed : (uint)Math.Pow(2, bitsPerPixel);
		}
		else if (bitsPerPixel == 16)
		{
			if (compression == Constants.BI_RGB)
				return colorsUsed;
			else if (compression == Constants.BI_BITFIELDS)
				return 3;  // RGB masks
		}
		else if (bitsPerPixel == 24 || bitsPerPixel == 32)
		{
			return colorsUsed;
		}
		else
		{
			ValidationReport.Error($"Invalid bit count: {bitsPerPixel}");
		}

		return 0;
	}
}
