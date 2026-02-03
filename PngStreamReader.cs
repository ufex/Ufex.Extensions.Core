using System;
using System.Collections.Generic;
using System.Linq;
using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Validation;
using Ufex.FileTypes.PNG.Data;

namespace Ufex.FileTypes.PNG;

public class PngStreamReader
{
	const string CHUNK_IHDR = "IHDR";
	
	private Stream _fileStream;
	internal List<Chunk> Chunks { get; init; }
	public Logger Log { get; set; }
	public ValidationReport ValidationReport { get; set; }
	public int ColorType { get; private set; }

	public PngStreamReader(Stream fileStream, Logger log, ValidationReport validationReport)
	{
		_fileStream = fileStream;
		Chunks = new List<Chunk>();
		Log = log;
		ValidationReport = validationReport;
	}

	public bool Read()
	{
		FileReader fr = new FileReader(_fileStream, Endian.Big);

		// Read the header
		byte[] header = fr.ReadBytes(8);
		if(!CompareArrays(header, PortableNetworkGraphic.SIGNATURE))
		{
			// Invalid header
			throw new Exception("Invalid PNG file signature.");
		}

		ColorType = -1;

		while(_fileStream.Position < _fileStream.Length)
		{
			UInt32 length = fr.ReadUInt32(); // Length of chunk data (exclusive of Length, Type, CRC)
			char[] chunkType = fr.ReadChars(4);
			Log.Info($"Reading PNG chunk of type: {new string(chunkType)} with length: {length}");

			// Rewind, so the chunk constructor can read the full chunk data
			_fileStream.Seek(-8, SeekOrigin.Current);

			string chunkTypeStr = new string(chunkType);

			Int64 startPos = _fileStream.Position + 8; // Position after length and type
			Chunk? newChunk = Chunk.CreateChunk(chunkTypeStr, fr, ColorType);
			if(newChunk == null)
			{
				throw new Exception("Failed to create PNG chunk of type: " + chunkTypeStr);
			}

			if(chunkTypeStr == CHUNK_IHDR)
			{
				IhdrChunk ihdr = (IhdrChunk)newChunk;
				ColorType = ihdr.ColorType;
			}

			Int64 endPos = _fileStream.Position;
			long adjustement = length - (endPos - startPos);
			if(adjustement > 0)
			{
				Log.Info($"Adjusting stream position by {adjustement} bytes for chunk type {chunkTypeStr}");
				_fileStream.Seek(length - (endPos - startPos), SeekOrigin.Current);
			}

			newChunk.CRC.Value = fr.ReadBytes(4);

			Chunks.Add(newChunk);
			Log.Info($"Position = {_fileStream.Position}");
		}

		return true;
	}
	
	private bool CompareArrays(byte[] array1, byte[] array2)
	{
		if(array1 == null || array2 == null)
			return false;

		return array1.SequenceEqual(array2);
	}
}