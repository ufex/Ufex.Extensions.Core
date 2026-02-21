using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Ufex.API;
using Ufex.API.Validation;
using Ufex.Extensions.Core.PDF.Data;

namespace Ufex.Extensions.Core.PDF;

/// <summary>
/// High-level PDF document reader. Orchestrates parsing of header, cross-reference table,
/// trailer, and all indirect objects in the file.
/// </summary>
internal class PdfStreamReader
{
	private readonly byte[] _data;
	private readonly PdfTokenizer _tokenizer;
	private readonly PdfParser _parser;
	private readonly ILogger _logger;
	private readonly ValidationReport _validationReport;

	/// <summary>Parsed file header</summary>
	public PdfHeader Header { get; private set; }

	/// <summary>All cross-reference entries</summary>
	public List<XRefEntry> XRefEntries { get; } = new();

	/// <summary>All parsed indirect objects, keyed by object number</summary>
	public Dictionary<int, IndirectObject> Objects { get; } = new();

	/// <summary>The trailer (or xref stream dict serving as trailer)</summary>
	public PdfTrailer Trailer { get; private set; }

	/// <summary>The startxref byte offset</summary>
	public long StartXRefOffset { get; private set; }

	/// <summary>Byte offset where the xref section begins</summary>
	public long XRefOffset { get; private set; }

	/// <summary>Whether the file uses an xref stream (vs traditional table)</summary>
	public bool UsesXRefStream { get; private set; }

	/// <summary>Total file size in bytes</summary>
	public long FileSize => _data.Length;

	/// <summary>Number of pages in the document (from the page tree)</summary>
	public int PageCount { get; private set; }

	/// <summary>Document info metadata</summary>
	public Dictionary<string, string> Metadata { get; } = new();

	public PdfStreamReader(byte[] data, ILogger logger, ValidationReport validationReport)
	{
		_data = data;
		_tokenizer = new PdfTokenizer(data);
		_parser = new PdfParser(_tokenizer);
		_logger = logger;
		_validationReport = validationReport;
		Header = new PdfHeader();
		Trailer = new PdfTrailer();
	}

	/// <summary>
	/// Reads and parses the entire PDF document.
	/// </summary>
	public bool Read()
	{
		try
		{
			// 1. Parse header
			_logger.LogInformation("Parsing PDF header");
			Header = _parser.ParseHeader();
			_logger.LogInformation("PDF version {Version}", Header.VersionString);

			// 2. Find startxref
			_logger.LogInformation("Finding startxref");
			StartXRefOffset = _parser.FindStartXRef();
			XRefOffset = StartXRefOffset;
			_logger.LogInformation("startxref at offset {Offset}", StartXRefOffset);

			// 3. Parse cross-reference section (table or stream)
			ParseXRefSection(StartXRefOffset);

			// 4. Parse all indirect objects from xref entries
			ParseAllObjects();

			// 5. Parse objects from object streams (if any)
			ParseObjectStreams();

			// 6. Extract document metadata
			ExtractMetadata();

			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error reading PDF file");
			_validationReport.Error($"Error reading PDF: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	/// Parses the cross-reference section and follows /Prev chains for incremental updates.
	/// </summary>
	private void ParseXRefSection(long offset)
	{
		var visitedOffsets = new HashSet<long>();

		while (offset >= 0 && !visitedOffsets.Contains(offset))
		{
			visitedOffsets.Add(offset);
			_logger.LogInformation("Parsing xref section at offset {Offset}", offset);

			List<XRefEntry> entries;
			PdfDictionary trailerDict;

			if (_parser.IsXRefStream(offset))
			{
				UsesXRefStream = true;
				(entries, trailerDict) = _parser.ParseXRefStream(offset);
				_logger.LogInformation("Xref stream: {Count} entries", entries.Count);
			}
			else
			{
				(entries, trailerDict) = _parser.ParseXRefTable(offset);
				_logger.LogInformation("Xref table: {Count} entries", entries.Count);
			}

			// Add entries that don't already exist (later entries take precedence)
			foreach (var entry in entries)
			{
				if (!XRefEntries.Any(e => e.ObjectNumber == entry.ObjectNumber))
					XRefEntries.Add(entry);
			}

			// Set trailer from the first (most recent) xref section
			if (visitedOffsets.Count == 1)
			{
				Trailer = new PdfTrailer
				{
					Offset = offset,
					Dictionary = trailerDict,
					StartXRefOffset = StartXRefOffset,
					UsesXRefStream = UsesXRefStream
				};
			}

			// Follow /Prev chain
			long? prev = trailerDict.GetInteger("Prev");
			if (prev.HasValue && prev.Value > 0)
			{
				_logger.LogInformation("Following /Prev to offset {Offset}", prev.Value);
				offset = prev.Value;
			}
			else
			{
				break;
			}
		}
	}

	/// <summary>
	/// Parses all indirect objects referenced by the cross-reference table.
	/// </summary>
	private void ParseAllObjects()
	{
		foreach (var entry in XRefEntries)
		{
			// Only parse in-use uncompressed objects (type 1)
			if (entry.Type != 1 || entry.Offset <= 0)
				continue;

			if (Objects.ContainsKey(entry.ObjectNumber))
				continue;

			try
			{
				var obj = _parser.ParseIndirectObjectAt(entry.Offset);
				Objects[obj.ObjectNumber] = obj;
				_logger.LogDebug("Parsed object {ObjNum} {Gen} ({Type}) at offset {Offset}",
					obj.ObjectNumber, obj.Generation, obj.DisplayName, entry.Offset);
			}
			catch (Exception ex)
			{
				_logger.LogWarning("Failed to parse object {ObjNum} at offset {Offset}: {Error}",
					entry.ObjectNumber, entry.Offset, ex.Message);
				_validationReport.Warning($"Failed to parse object {entry.ObjectNumber} at offset {entry.Offset}: {ex.Message}");
			}
		}
	}

	/// <summary>
	/// Parses objects stored within object streams (type 2 xref entries).
	/// </summary>
	private void ParseObjectStreams()
	{
		// Find all type 2 entries and group by stream object number
		var compressedEntries = XRefEntries
			.Where(e => e.Type == 2)
			.GroupBy(e => (int)e.Offset) // For type 2, Offset field stores the stream object number
			.ToList();

		foreach (var group in compressedEntries)
		{
			int streamObjNum = group.Key;
			if (!Objects.TryGetValue(streamObjNum, out var streamObj))
				continue;
			if (streamObj.Value is not PdfStream objStream)
				continue;

			try
			{
				byte[]? decoded = _parser.DecodeStreamData(objStream);
				if (decoded == null || decoded.Length == 0) continue;

				int n = (int)(objStream.Dict.GetInteger("N") ?? 0);
				int first = (int)(objStream.Dict.GetInteger("First") ?? 0);

				// Parse header region: pairs of (objNum offset)
				var headerTokenizer = new PdfTokenizer(decoded);
				var headerParser = new PdfParser(headerTokenizer);
				var objEntries = new List<(int ObjNum, int Offset)>();

				for (int i = 0; i < n; i++)
				{
					var numTok = headerTokenizer.ReadToken();
					var offTok = headerTokenizer.ReadToken();
					if (int.TryParse(numTok.Value, out int objNum) && int.TryParse(offTok.Value, out int off))
						objEntries.Add((objNum, off));
				}

				// Parse each object in the object stream
				foreach (var (objNum, off) in objEntries)
				{
					if (Objects.ContainsKey(objNum)) continue;

					try
					{
						var innerTokenizer = new PdfTokenizer(decoded);
						var innerParser = new PdfParser(innerTokenizer);
						innerTokenizer.Position = first + off;
						var value = innerParser.ParseObject();

						Objects[objNum] = new IndirectObject(objNum, 0, streamObj.Offset, value)
						{
							EndOffset = streamObj.EndOffset
						};
					}
					catch (Exception ex)
					{
						_logger.LogDebug("Failed to parse compressed object {ObjNum}: {Error}", objNum, ex.Message);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning("Failed to parse object stream {ObjNum}: {Error}", streamObjNum, ex.Message);
			}
		}
	}

	/// <summary>
	/// Extracts document metadata from the document info dictionary and catalog.
	/// </summary>
	private void ExtractMetadata()
	{
		// Get page count from catalog → pages
		var rootRef = Trailer.Root;
		if (rootRef != null && Objects.TryGetValue(rootRef.ObjectNumber, out var catalogObj))
		{
			var catalogDict = catalogObj.Dictionary;
			if (catalogDict != null)
			{
				var pagesRef = catalogDict.GetReference("Pages");
				if (pagesRef != null && Objects.TryGetValue(pagesRef.ObjectNumber, out var pagesObj))
				{
					var pagesDict = pagesObj.Dictionary;
					PageCount = (int)(pagesDict?.GetInteger("Count") ?? 0);
				}
			}
		}

		// Get info dictionary
		var infoRef = Trailer.Info;
		if (infoRef != null && Objects.TryGetValue(infoRef.ObjectNumber, out var infoObj))
		{
			var infoDict = infoObj.Dictionary;
			if (infoDict != null)
			{
				ExtractStringMetadata(infoDict, "Title");
				ExtractStringMetadata(infoDict, "Author");
				ExtractStringMetadata(infoDict, "Subject");
				ExtractStringMetadata(infoDict, "Creator");
				ExtractStringMetadata(infoDict, "Producer");
				ExtractStringMetadata(infoDict, "Keywords");
				ExtractDateMetadata(infoDict, "CreationDate");
				ExtractDateMetadata(infoDict, "ModDate");
			}
		}
	}

	private void ExtractStringMetadata(PdfDictionary dict, string key)
	{
		var val = dict.Get(key);
		if (val is PdfString str && str.TextValue.Length > 0)
			Metadata[key] = str.TextValue;
		else if (val is PdfHexString hex && hex.RawBytes.Length > 0)
			Metadata[key] = System.Text.Encoding.UTF8.GetString(hex.RawBytes);
	}

	private void ExtractDateMetadata(PdfDictionary dict, string key)
	{
		var val = dict.Get(key);
		string? dateStr = null;
		if (val is PdfString str)
			dateStr = str.TextValue;
		else if (val is PdfHexString hex)
			dateStr = System.Text.Encoding.UTF8.GetString(hex.RawBytes);

		if (dateStr == null) return;

		// PDF date format: D:YYYYMMDDHHmmSSOHH'mm'
		if (dateStr.StartsWith("D:"))
			dateStr = dateStr.Substring(2);

		// Try to extract at least YYYY-MM-DD HH:mm:ss
		if (dateStr.Length >= 14)
		{
			try
			{
				string formatted = $"{dateStr.Substring(0, 4)}-{dateStr.Substring(4, 2)}-{dateStr.Substring(6, 2)} " +
					$"{dateStr.Substring(8, 2)}:{dateStr.Substring(10, 2)}:{dateStr.Substring(12, 2)}";
				Metadata[key] = formatted;
			}
			catch
			{
				Metadata[key] = dateStr;
			}
		}
		else if (dateStr.Length >= 8)
		{
			try
			{
				Metadata[key] = $"{dateStr.Substring(0, 4)}-{dateStr.Substring(4, 2)}-{dateStr.Substring(6, 2)}";
			}
			catch
			{
				Metadata[key] = dateStr;
			}
		}
		else
		{
			Metadata[key] = dateStr;
		}
	}

	/// <summary>
	/// Resolves an indirect reference to the actual object.
	/// </summary>
	public PdfObject? ResolveReference(PdfReference reference)
	{
		return Objects.TryGetValue(reference.ObjectNumber, out var obj) ? obj.Value : null;
	}

	/// <summary>
	/// Resolves any PdfReference to its underlying object, or returns the object as-is.
	/// </summary>
	public PdfObject Resolve(PdfObject obj)
	{
		if (obj is PdfReference r)
			return ResolveReference(r) ?? PdfNull.Instance;
		return obj;
	}
}