using System;
using System.Collections.Generic;
using System.IO;

using Ufex.API;
using Ufex.API.Validation;

using Ufex.Extensions.Core.ISOBMFF.Data;

namespace Ufex.Extensions.Core.ISOBMFF;

/// <summary>
/// Reads an ISOBMFF/QTFF stream and populates a list of top-level boxes.
/// </summary>
public class BoxStreamReader
{
	private Stream _fileStream;

	/// <summary>
	/// The top-level boxes parsed from the file.
	/// </summary>
	internal List<Box> Boxes { get; init; }

	public Logger Log { get; set; }
	public ValidationReport ValidationReport { get; set; }

	public BoxStreamReader(Stream fileStream, Logger log, ValidationReport validationReport)
	{
		_fileStream = fileStream;
		Boxes = [];
		Log = log;
		ValidationReport = validationReport;
	}

	public bool Read()
	{
		FileReader fr = new FileReader(_fileStream, Endian.Big);

		while (_fileStream.Position + 8 <= _fileStream.Length)
		{
			Int64 boxStart = _fileStream.Position;

			// Peek at the size and type
			UInt32 peekSize = fr.ReadUInt32();
			Byte[] peekType = fr.ReadBytes(4);
			string typeStr = System.Text.Encoding.ASCII.GetString(peekType);

			// Determine actual box size
			UInt64 actualSize;
			if (peekSize == 1)
			{
				actualSize = fr.ReadUInt64();
				_fileStream.Seek(boxStart, SeekOrigin.Begin);
			}
			else if (peekSize == 0)
			{
				actualSize = (UInt64)(_fileStream.Length - boxStart);
				_fileStream.Seek(boxStart, SeekOrigin.Begin);
			}
			else
			{
				actualSize = peekSize;
				_fileStream.Seek(boxStart, SeekOrigin.Begin);
			}

			Log.Info($"Reading box '{typeStr}' at offset {boxStart} with size {actualSize}");

			Int64 boxEnd = boxStart + (Int64)actualSize;
			boxEnd = Math.Min(boxEnd, _fileStream.Length);

			Box box = Box.CreateBox(typeStr, fr, _fileStream.Length);
			Boxes.Add(box);

			// Ensure stream is positioned at the end of this box
			if (_fileStream.Position != boxEnd)
			{
				Log.Info($"Adjusting stream position from {_fileStream.Position} to {boxEnd}");
				_fileStream.Seek(boxEnd, SeekOrigin.Begin);
			}
		}

		return true;
	}
}
