using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;

using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Validation;

using Ufex.Extensions.Core.RIFF.Data;

namespace Ufex.Extensions.Core.RIFF;

public class RiffStreamReader
{	
	/// <summary>
	/// The RIFF file is read from this stream. 
	/// It is the responsibility of the caller to open and close the stream.
	/// </summary>
	private Stream _fileStream;

	/// <summary>
	/// The top-level chunks in the RIFF file.
	/// There should only be one RIFF chunk, but we use a list to be more flexible and 
	/// allow for non-standard files that may have multiple RIFF chunks.
	/// </summary>
	internal List<Chunk> Chunks { get; init; }
	public Logger Log { get; set; }
	public ValidationReport ValidationReport { get; set; }

	public RiffStreamReader(Stream fileStream, Logger log, ValidationReport validationReport)
	{
		_fileStream = fileStream;
		Chunks = new List<Chunk>();
		Log = log;
		ValidationReport = validationReport;
	}

	public bool Read()
	{
		FileReader fr = new FileReader(_fileStream, Endian.Little);

		while(_fileStream.Position + 8 <= _fileStream.Length)
		{
			byte[] chunkID = fr.ReadBytes(4);
			UInt32 size = fr.ReadUInt32();
			if(System.Text.Encoding.ASCII.GetString(chunkID) != "RIFF")
			{
				ValidationReport.Error($"Expected RIFF chunk at position {_fileStream.Position - 8}, but found {System.Text.Encoding.ASCII.GetString(chunkID)}.");
				return false;
			}
			byte[] chunkFormat = fr.ReadBytes(4);
			Log.LogInformation($"Reading chunk of type: {System.Text.Encoding.ASCII.GetString(chunkID)} with length: {size}");

			// Rewind, so the chunk constructor can read the full chunk data
			_fileStream.Seek(-12, SeekOrigin.Current);

			Int64 startPos = _fileStream.Position + 8; // Position after length and type
			Chunk? newChunk = Chunk.CreateRiffChunk(chunkFormat, fr);
			if(newChunk.GetType() == typeof(RiffChunk))
			{
				ValidationReport.Warning($"Unsupported RIFF format: {System.Text.Encoding.ASCII.GetString(chunkID)}, skipping {size} bytes of data.");
			}

			Int64 endPos = _fileStream.Position;
			long adjustment = size - (endPos - startPos);
			if(adjustment > 0)
			{
				Log.LogInformation($"Adjusting stream position by {adjustment} bytes for chunk type {System.Text.Encoding.ASCII.GetString(chunkID)}");
				_fileStream.Seek(adjustment, SeekOrigin.Current);
			}

			Chunks.Add(newChunk);
			Log.LogInformation($"Position = {_fileStream.Position}");
		}

		return true;
	}

}