using Ufex.API;
using Ufex.API.Validation;
using Ufex.Extensions.Core.BMP.Data;

namespace Ufex.Extensions.Core.BMP;

/// <summary>
/// Reads and parses Windows Icon (ICO) files
/// </summary>
public class IcoStreamReader
{
	private readonly Stream _fileStream;

	public Logger Log { get; set; }
	public ValidationReport ValidationReport { get; set; }

	/// <summary>
	/// The icon directory header
	/// </summary>
	internal IconDir? Header { get; private set; }

	/// <summary>
	/// The icon directory entries
	/// </summary>
	internal List<IconDirEntry> Entries { get; private set; } = new();

	/// <summary>
	/// The icon images
	/// </summary>
	internal List<IconImage> Images { get; private set; } = new();

	public IcoStreamReader(Stream fileStream, Logger log, ValidationReport validationReport)
	{
		_fileStream = fileStream;
		Log = log;
		ValidationReport = validationReport;
	}

	public bool Read()
	{
		FileReader fr = new FileReader(_fileStream, Endian.Little);

		// Read Icon Directory header
		Header = new IconDir(fr);
		Log.Info($"Read IconDir at offset {Header.Offset}, Count={Header.Count}");

		// Validate header
		if (Header.Reserved != 0)
			ValidationReport.Warning("idReserved should be zero");

		if (Header.Type != Signatures.ICO_TYPE && Header.Type != Signatures.CUR_TYPE)
		{
			ValidationReport.Error($"Invalid icon type: {Header.Type} (expected 1 for ICO or 2 for CUR)");
			throw new Exception($"Invalid icon type: {Header.Type}");
		}

		// Read directory entries
		for (int i = 0; i < Header.Count; i++)
		{
			var entry = new IconDirEntry(fr);
			Entries.Add(entry);
			Log.Info($"Read IconDirEntry[{i}] at offset {entry.Offset}: {entry.ActualWidth}x{entry.ActualHeight}");

			if (entry.Reserved != 0)
				ValidationReport.Warning($"Entry[{i}] bReserved should be zero");
		}

		// Read icon images
		for (int i = 0; i < Header.Count; i++)
		{
			var entry = Entries[i];
			_fileStream.Position = entry.ImageOffset;

			// Check if it's a PNG (some ICO files embed PNG data)
			byte firstByte = fr.ReadByte();
			_fileStream.Seek(-1, SeekOrigin.Current);

			if (firstByte == 0x89)  // PNG signature starts with 0x89
			{
				Log.Info($"Image[{i}] appears to be PNG format - skipping detailed parsing");
				ValidationReport.Info($"Image[{i}] is embedded PNG format");
				continue;
			}

			try
			{
				var image = new IconImage(fr, i, entry);
				Images.Add(image);
				Log.Info($"Read IconImage[{i}] at offset {image.Offset}");
			}
			catch (Exception ex)
			{
				ValidationReport.Warning($"Failed to parse image[{i}]: {ex.Message}");
				Log.Info($"Error reading IconImage[{i}]: {ex.Message}");
			}
		}

		string fileType = Header.Type == Signatures.ICO_TYPE ? "ICO" : "CUR";
		ValidationReport.Info($"Successfully parsed {fileType}: {Header.Count} images");

		return true;
	}
}
