using System.IO.Compression;
using Ufex.Extensions.Core.PDF.Data;

namespace Ufex.Extensions.Core.PDF;

/// <summary>
/// Parses PDF objects from tokenized data. Handles all PDF object types,
/// indirect objects, cross-reference tables/streams, and trailer dictionaries.
/// </summary>
internal class PdfParser
{
	private readonly PdfTokenizer _tokenizer;

	public PdfParser(PdfTokenizer tokenizer)
	{
		_tokenizer = tokenizer;
	}

	/// <summary>
	/// Parses and returns the file header (%PDF-x.y and optional binary marker).
	/// </summary>
	public PdfHeader ParseHeader()
	{
		_tokenizer.Position = 0;

		// Find %PDF- header (may not be at byte 0 in linearized PDFs)
		int headerPos = 0;
		string rawHeader = "";
		for (int i = 0; i < Math.Min(1024, _tokenizer.Length); i++)
		{
			if (_tokenizer.Length - i >= 5)
			{
				var bytes = _tokenizer.GetBytes(i, 5);
				if (bytes.SequenceEqual(Signatures.HeaderMagic))
				{
					headerPos = i;
					_tokenizer.Position = i;
					rawHeader = _tokenizer.ReadLineContent();
					_tokenizer.SkipEol();
					break;
				}
			}
		}

		if (string.IsNullOrEmpty(rawHeader) || rawHeader.Length < 8)
			throw new InvalidDataException("Could not find PDF header (%PDF-x.y)");

		// Parse version: %PDF-M.m
		int major = rawHeader[5] - '0';
		int minor = rawHeader[7] - '0';

		// Check for binary marker (line of 4+ bytes with values > 127)
		bool hasBinaryMarker = false;
		int markerStart = _tokenizer.Position;
		if (_tokenizer.Position < _tokenizer.Length)
		{
			byte? peek = _tokenizer.PeekByte();
			if (peek == (byte)'%')
			{
				int savedPos = _tokenizer.Position;
				_tokenizer.Position++;
				int highCount = 0;
				while (_tokenizer.Position < _tokenizer.Length)
				{
					byte? b = _tokenizer.PeekByte();
					if (b == null || b == 13 || b == 10) break;
					if (b > 127) highCount++;
					_tokenizer.Position++;
				}
				hasBinaryMarker = highCount >= 4;
				if (!hasBinaryMarker)
					_tokenizer.Position = savedPos;
				else
					_tokenizer.SkipEol();
			}
		}

		int headerLength = _tokenizer.Position - headerPos;

		return new PdfHeader
		{
			Offset = headerPos,
			MajorVersion = major,
			MinorVersion = minor,
			RawHeader = rawHeader,
			HasBinaryMarker = hasBinaryMarker,
			Length = headerLength
		};
	}

	/// <summary>
	/// Finds the startxref offset by scanning from the end of the file.
	/// </summary>
	public long FindStartXRef()
	{
		// Search last 1024 bytes for "startxref"
		int searchStart = Math.Max(0, _tokenizer.Length - 1024);
		int pos = _tokenizer.FindLastOccurrence("startxref", _tokenizer.Length - 1);
		if (pos < 0)
			throw new InvalidDataException("Could not find startxref keyword");

		_tokenizer.Position = pos + 9; // length of "startxref"
		_tokenizer.SkipWhitespaceAndComments();
		string offsetStr = _tokenizer.ReadLineContent().Trim();
		if (!long.TryParse(offsetStr, out long offset))
			throw new InvalidDataException($"Invalid startxref offset: {offsetStr}");

		return offset;
	}

	/// <summary>
	/// Parses a traditional cross-reference table starting at the given offset.
	/// Returns the parsed entries and the trailer dictionary.
	/// </summary>
	public (List<XRefEntry> Entries, PdfDictionary Trailer) ParseXRefTable(long offset)
	{
		_tokenizer.Position = (int)offset;
		var entries = new List<XRefEntry>();

		// Expect 'xref' keyword
		string keyword = _tokenizer.ReadLineContent().Trim();
		if (keyword != "xref")
			throw new InvalidDataException($"Expected 'xref' at offset {offset}, got '{keyword}'");
		_tokenizer.SkipEol();

		// Read subsections: "startObj count"
		while (_tokenizer.Position < _tokenizer.Length)
		{
			_tokenizer.SkipWhitespaceAndComments();
			if (_tokenizer.Position >= _tokenizer.Length) break;

			// Check if we've reached the trailer keyword
			int savedPos = _tokenizer.Position;
			string line = _tokenizer.ReadLineContent().Trim();
			if (line.StartsWith("trailer"))
			{
				_tokenizer.Position = savedPos + 7; // length of "trailer"
				break;
			}

			// Parse subsection header: "startObj count"
			var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length < 2 || !int.TryParse(parts[0], out int startObj) || !int.TryParse(parts[1], out int count))
			{
				_tokenizer.Position = savedPos;
				break;
			}
			_tokenizer.SkipEol();

			// Read entries — each is exactly 20 bytes: "oooooooooo ggggg n \n"
			for (int i = 0; i < count; i++)
			{
				string entryLine = _tokenizer.ReadLine();
				if (entryLine.Length < 18) continue;

				string offsetStr = entryLine.Substring(0, 10).Trim();
				string genStr = entryLine.Substring(11, 5).Trim();
				char marker = entryLine.Length > 17 ? entryLine[17] : 'f';

				if (long.TryParse(offsetStr, out long entryOffset) && int.TryParse(genStr, out int gen))
				{
					entries.Add(new XRefEntry
					{
						ObjectNumber = startObj + i,
						Generation = gen,
						Offset = entryOffset,
						Type = marker == 'n' ? 1 : 0
					});
				}
			}
		}

		// Parse trailer dictionary
		_tokenizer.SkipWhitespaceAndComments();
		var trailerDict = ParseObject() as PdfDictionary ?? new PdfDictionary();

		return (entries, trailerDict);
	}

	/// <summary>
	/// Determines whether the xref at the given offset is a traditional table or an xref stream.
	/// </summary>
	public bool IsXRefStream(long offset)
	{
		_tokenizer.Position = (int)offset;
		_tokenizer.SkipWhitespaceAndComments();
		// If it starts with 'xref', it's a traditional table
		int savedPos = _tokenizer.Position;
		string firstContent = _tokenizer.ReadLineContent().Trim();
		_tokenizer.Position = savedPos;
		return !firstContent.StartsWith("xref");
	}

	/// <summary>
	/// Parses a cross-reference stream object at the given offset.
	/// Returns the parsed entries and the stream dictionary (which serves as the trailer).
	/// </summary>
	public (List<XRefEntry> Entries, PdfDictionary Trailer) ParseXRefStream(long offset)
	{
		// The xref stream is an indirect object containing a stream
		var obj = ParseIndirectObjectAt(offset);
		if (obj.Value is not PdfStream stream)
			throw new InvalidDataException($"Expected xref stream object at offset {offset}");

		var dict = stream.Dict;
		var entries = new List<XRefEntry>();

		// Decode the stream data
		byte[]? data = DecodeStreamData(stream);
		if (data == null || data.Length == 0)
			return (entries, dict);

		// Get /W array (field widths)
		var wArray = dict.GetArray("W");
		if (wArray == null || wArray.Count < 3)
			throw new InvalidDataException("XRef stream missing /W array");

		int w0 = (int)(wArray[0] as PdfInteger)!.Value;
		int w1 = (int)(wArray[1] as PdfInteger)!.Value;
		int w2 = (int)(wArray[2] as PdfInteger)!.Value;
		int entrySize = w0 + w1 + w2;

		// Get /Index array (defaults to [0 Size])
		int[] index;
		var indexArray = dict.GetArray("Index");
		if (indexArray != null)
		{
			index = new int[indexArray.Count];
			for (int i = 0; i < indexArray.Count; i++)
				index[i] = (int)((PdfInteger)indexArray[i]).Value;
		}
		else
		{
			int size = (int)(dict.GetInteger("Size") ?? 0);
			index = [0, size];
		}

		// Parse entries
		int dataPos = 0;
		for (int s = 0; s < index.Length; s += 2)
		{
			int startObj = index[s];
			int count = index[s + 1];
			for (int i = 0; i < count && dataPos + entrySize <= data.Length; i++)
			{
				long f0 = ReadXRefField(data, dataPos, w0);
				long f1 = ReadXRefField(data, dataPos + w0, w1);
				long f2 = ReadXRefField(data, dataPos + w0 + w1, w2);
				dataPos += entrySize;

				// Default type is 1 if w0 == 0
				int type = w0 > 0 ? (int)f0 : 1;
				entries.Add(new XRefEntry
				{
					ObjectNumber = startObj + i,
					Type = type,
					Offset = type == 1 ? f1 : 0,
					Generation = type == 1 ? (int)f2 : 0,
					StreamIndex = type == 2 ? (int)f2 : 0,
				});
			}
		}

		return (entries, dict);
	}

	/// <summary>
	/// Parses an indirect object at the given byte offset.
	/// Format: ObjNum Gen obj [object] endobj
	/// </summary>
	public IndirectObject ParseIndirectObjectAt(long offset)
	{
		_tokenizer.Position = (int)offset;
		_tokenizer.SkipWhitespaceAndComments();

		// Read "ObjNum Gen obj"
		var numToken = _tokenizer.ReadToken();
		var genToken = _tokenizer.ReadToken();
		var objKeyword = _tokenizer.ReadToken();

		if (objKeyword.Type != PdfTokenType.Keyword || objKeyword.Value != "obj")
			throw new InvalidDataException($"Expected 'obj' keyword at offset {offset}, got {objKeyword}");

		int objNum = int.Parse(numToken.Value);
		int gen = int.Parse(genToken.Value);

		// Parse the object value
		PdfObject value = ParseObject();

		// Check for stream
		_tokenizer.SkipWhitespaceAndComments();
		int peekPos = _tokenizer.Position;
		var nextToken = _tokenizer.ReadToken();

		if (nextToken.Type == PdfTokenType.Keyword && nextToken.Value == "stream" && value is PdfDictionary streamDict)
		{
			value = ParseStreamData(streamDict);
		}
		else
		{
			_tokenizer.Position = peekPos;
		}

		// Find endobj
		_tokenizer.SkipWhitespaceAndComments();
		peekPos = _tokenizer.Position;
		nextToken = _tokenizer.ReadToken();
		long endOffset = _tokenizer.Position;
		if (nextToken.Type != PdfTokenType.Keyword || nextToken.Value != "endobj")
		{
			// Tolerate missing endobj
			endOffset = peekPos;
		}

		var indirectObj = new IndirectObject(objNum, gen, offset, value)
		{
			EndOffset = endOffset
		};
		return indirectObj;
	}

	/// <summary>
	/// Parses a PDF object from the current tokenizer position.
	/// </summary>
	public PdfObject ParseObject()
	{
		_tokenizer.SkipWhitespaceAndComments();
		var token = _tokenizer.ReadToken();

		switch (token.Type)
		{
			case PdfTokenType.Eof:
				return PdfNull.Instance;

			case PdfTokenType.Keyword:
				return token.Value switch
				{
					"true" => new PdfBoolean(true),
					"false" => new PdfBoolean(false),
					"null" => PdfNull.Instance,
					_ => PdfNull.Instance // Unknown keyword, treat as null
				};

			case PdfTokenType.Number:
				return ParseNumberOrReference(token);

			case PdfTokenType.Name:
				return new PdfName(token.Value);

			case PdfTokenType.LiteralString:
				return new PdfString(token.Bytes ?? []);

			case PdfTokenType.HexString:
				return new PdfHexString(token.Bytes ?? []);

			case PdfTokenType.ArrayBegin:
				return ParseArray();

			case PdfTokenType.DictBegin:
				return ParseDictionary();

			default:
				return PdfNull.Instance;
		}
	}

	#region Private parsing methods

	/// <summary>
	/// A number token might be part of an indirect reference: "10 0 R".
	/// Peek ahead to check.
	/// </summary>
	private PdfObject ParseNumberOrReference(PdfToken numberToken)
	{
		int savedPos = _tokenizer.Position;

		// Try to parse "gen R"
		var secondToken = _tokenizer.PeekToken();
		if (secondToken.Type == PdfTokenType.Number)
		{
			_tokenizer.ReadToken(); // consume second number
			var thirdToken = _tokenizer.PeekToken();
			if (thirdToken.Type == PdfTokenType.Keyword && thirdToken.Value == "R")
			{
				_tokenizer.ReadToken(); // consume R
				if (int.TryParse(numberToken.Value, out int objNum) && int.TryParse(secondToken.Value, out int gen))
					return new PdfReference(objNum, gen);
			}
			// Not a reference — restore position after second token was consumed
			_tokenizer.Position = savedPos;
		}

		// Just a number
		if (numberToken.Value.Contains('.'))
		{
			if (double.TryParse(numberToken.Value, System.Globalization.NumberStyles.Float,
				System.Globalization.CultureInfo.InvariantCulture, out double d))
				return new PdfReal(d);
		}
		if (long.TryParse(numberToken.Value, out long l))
			return new PdfInteger(l);

		return new PdfReal(0);
	}

	private PdfArray ParseArray()
	{
		var array = new PdfArray();
		while (true)
		{
			_tokenizer.SkipWhitespaceAndComments();
			var peek = _tokenizer.PeekToken();
			if (peek.Type == PdfTokenType.ArrayEnd)
			{
				_tokenizer.ReadToken(); // consume ]
				break;
			}
			if (peek.Type == PdfTokenType.Eof)
				break;
			array.Items.Add(ParseObject());
		}
		return array;
	}

	private PdfDictionary ParseDictionary()
	{
		var dict = new PdfDictionary();
		while (true)
		{
			_tokenizer.SkipWhitespaceAndComments();
			var peek = _tokenizer.PeekToken();
			if (peek.Type == PdfTokenType.DictEnd)
			{
				_tokenizer.ReadToken(); // consume >>
				break;
			}
			if (peek.Type == PdfTokenType.Eof)
				break;

			// Key must be a name
			var keyToken = _tokenizer.ReadToken();
			if (keyToken.Type != PdfTokenType.Name)
				continue;

			// Value is any object
			var value = ParseObject();
			dict.Entries[keyToken.Value] = value;
		}
		return dict;
	}

	/// <summary>
	/// Parses stream data after the 'stream' keyword has been consumed.
	/// The stream keyword must be followed by a single EOL (CR, LF, or CRLF).
	/// </summary>
	private PdfStream ParseStreamData(PdfDictionary dict)
	{
		// Skip single end-of-line after 'stream' keyword
		_tokenizer.SkipEol();

		long dataOffset = _tokenizer.Position;
		long length = dict.GetInteger("Length") ?? 0;

		// If length is valid, skip ahead
		if (length > 0 && dataOffset + length <= _tokenizer.Length)
		{
			_tokenizer.Position = (int)(dataOffset + length);
		}
		else
		{
			// Fallback: scan for endstream
			length = ScanForEndStream(dataOffset);
		}

		// Skip any whitespace before endstream
		_tokenizer.SkipWhitespaceAndComments();
		int endstreamCheck = _tokenizer.Position;
		var token = _tokenizer.ReadToken();
		if (token.Type != PdfTokenType.Keyword || token.Value != "endstream")
		{
			// Tolerate missing endstream
			_tokenizer.Position = endstreamCheck;
		}

		return new PdfStream(dict, dataOffset, length);
	}

	/// <summary>
	/// Scans forward from the given position to find 'endstream', returning data length.
	/// </summary>
	private long ScanForEndStream(long dataOffset)
	{
		int pos = (int)dataOffset;
		int endstreamPos = _tokenizer.FindLastOccurrence("endstream", _tokenizer.Length - 1);
		if (endstreamPos > pos)
			return endstreamPos - pos;

		// Fallback
		return 0;
	}

	/// <summary>
	/// Decodes stream data, handling FlateDecode filter.
	/// </summary>
	public byte[]? DecodeStreamData(PdfStream stream)
	{
		var rawData = _tokenizer.GetBytes((int)stream.DataOffset, (int)stream.DataLength);
		if (rawData.Length == 0) return rawData;

		var filter = stream.Dict.GetName("Filter");
		if (filter == null)
		{
			// Check for filter array
			var filterArray = stream.Dict.GetArray("Filter");
			if (filterArray != null && filterArray.Count > 0 && filterArray[0] is PdfName name)
				filter = name.Value;
		}

		if (filter == null)
			return rawData;

		if (filter == "FlateDecode" || filter == "Fl")
		{
			try
			{
				return DecompressFlate(rawData);
			}
			catch
			{
				return rawData; // Return raw if decompression fails
			}
		}

		// Unsupported filter — return raw
		return rawData;
	}

	/// <summary>
	/// Decompresses FlateDecode (zlib) data.
	/// </summary>
	private static byte[] DecompressFlate(byte[] data)
	{
		// Skip zlib header (2 bytes) if present
		int offset = 0;
		if (data.Length >= 2)
		{
			// Check for zlib header: first byte CMF, second byte FLG
			int cmf = data[0];
			int flg = data[1];
			if ((cmf * 256 + flg) % 31 == 0 && (cmf & 0x0F) == 8)
			{
				offset = 2;
			}
		}

		using var input = new MemoryStream(data, offset, data.Length - offset);
		using var deflate = new DeflateStream(input, CompressionMode.Decompress);
		using var output = new MemoryStream();
		deflate.CopyTo(output);
		return output.ToArray();
	}

	/// <summary>
	/// Reads a variable-width big-endian integer from xref stream data.
	/// </summary>
	private static long ReadXRefField(byte[] data, int offset, int width)
	{
		if (width == 0) return 0;
		long result = 0;
		for (int i = 0; i < width && offset + i < data.Length; i++)
		{
			result = (result << 8) | data[offset + i];
		}
		return result;
	}

	#endregion
}
