using System.Text;
using Microsoft.Extensions.Logging;

using Ufex.API;
using Ufex.API.Validation;

using Ufex.Extensions.Core.INI.Data;

namespace Ufex.Extensions.Core.INI;

/// <summary>
/// Parses INI files according to the Core INI specification.
/// </summary>
public class IniStreamReader
{
	private readonly Stream _fileStream;
	private readonly Logger _log;
	private readonly ValidationReport _validationReport;

	/// <summary>
	/// Parsed sections (including implicit global section if present)
	/// </summary>
	public List<Section> Sections { get; } = new();

	/// <summary>
	/// All parsed lines in file order
	/// </summary>
	public List<Line> Lines { get; } = new();

	/// <summary>
	/// Lines that failed to parse as valid Core INI
	/// </summary>
	public List<InvalidLine> InvalidLines { get; } = new();

	/// <summary>
	/// Total number of properties across all sections
	/// </summary>
	public int PropertyCount { get; private set; }

	/// <summary>
	/// Total number of comment lines
	/// </summary>
	public int CommentCount { get; private set; }

	/// <summary>
	/// Detected text encoding of the file
	/// </summary>
	public Encoding? DetectedEncoding { get; private set; }

	/// <summary>
	/// Whether a UTF-8 BOM was present
	/// </summary>
	public bool HasBom { get; private set; }

	public IniStreamReader(Stream fileStream, Logger log, ValidationReport validationReport)
	{
		_fileStream = fileStream;
		_log = log;
		_validationReport = validationReport;
	}

	/// <summary>
	/// Reads and parses the INI file.
	/// </summary>
	/// <returns>True if parsing completed (may still have validation warnings/errors)</returns>
	public bool Read()
	{
		_fileStream.Position = 0;

		// Detect encoding and BOM
		DetectedEncoding = DetectEncoding(_fileStream, out bool hasBom);
		HasBom = hasBom;

		using var reader = new StreamReader(_fileStream, DetectedEncoding, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

		Section? currentSection = null;
		int lineNumber = 0;
		long currentOffset = hasBom ? GetBomLength(DetectedEncoding) : 0;

		while (!reader.EndOfStream)
		{
			string? rawLine = reader.ReadLine();
			if (rawLine == null)
				break;

			// Calculate line length in bytes (including newline, approximate for text files)
			int lineLength = DetectedEncoding.GetByteCount(rawLine);
			// Account for line terminator (we don't know if CRLF or LF, estimate 1-2 bytes)
			int terminatorLength = reader.EndOfStream ? 0 : (DetectedEncoding == Encoding.Unicode || DetectedEncoding == Encoding.BigEndianUnicode ? 2 : 1);
			if (!reader.EndOfStream)
			{
				// Peek ahead to check for CRLF vs LF - simplified: assume typical case
				lineLength += terminatorLength;
			}

			var line = ParseLine(rawLine, lineNumber, currentOffset, lineLength);
			Lines.Add(line);

			switch (line)
			{
				case SectionLine sectionLine:
					// Start a new section or continue an existing one
					currentSection = GetOrCreateSection(sectionLine.SectionName);
					if (currentSection.HeaderLine == null)
					{
						currentSection.HeaderLine = sectionLine;
					}
					else
					{
						// Duplicate section header - per spec, this continues the section
						_validationReport.Info($"Line {lineNumber + 1}: Section [{sectionLine.SectionName}] continued (duplicate header)");
					}
					currentSection.Lines.Add(line);
					break;

				case PropertyLine propertyLine:
					// Add to current section (or create global section)
					if (currentSection == null)
					{
						currentSection = GetOrCreateGlobalSection();
					}
					var property = new Property(propertyLine.Key, propertyLine.Value, propertyLine);
					currentSection.Properties.Add(property);
					currentSection.Lines.Add(line);
					PropertyCount++;
					break;

				case CommentLine:
					CommentCount++;
					currentSection?.Lines.Add(line);
					break;

				case BlankLine:
					currentSection?.Lines.Add(line);
					break;

				case InvalidLine invalidLine:
					InvalidLines.Add(invalidLine);
					_validationReport.Warning($"Line {lineNumber + 1}: {invalidLine.Reason}");
					currentSection?.Lines.Add(line);
					break;
			}

			currentOffset += lineLength;
			lineNumber++;
		}

		// Report summary
		_validationReport.Info($"Parsed {Sections.Count} section(s), {PropertyCount} property(ies), {CommentCount} comment(s)");
		if (InvalidLines.Count > 0)
		{
			_validationReport.Warning($"{InvalidLines.Count} invalid line(s) found");
		}

		return true;
	}

	/// <summary>
	/// Parses a single line according to Core INI specification.
	/// </summary>
	private Line ParseLine(string rawLine, int lineNumber, long offset, int length)
	{
		// Trim leading whitespace for classification
		var trimmedStart = rawLine.TrimStart(' ', '\t');

		// Blank line: only whitespace
		if (string.IsNullOrEmpty(trimmedStart))
		{
			return new BlankLine(lineNumber, offset, length, rawLine);
		}

		char firstChar = trimmedStart[0];

		// Comment line: starts with ; or #
		if (firstChar == ';' || firstChar == '#')
		{
			string commentText = trimmedStart.Length > 1
				? trimmedStart.Substring(1).TrimStart(' ', '\t')
				: string.Empty;
			return new CommentLine(lineNumber, offset, length, rawLine, firstChar, commentText);
		}

		// Section header: starts with [
		if (firstChar == '[')
		{
			return ParseSectionLine(rawLine, trimmedStart, lineNumber, offset, length);
		}

		// Property line: contains =
		int equalsIndex = rawLine.IndexOf('=');
		if (equalsIndex > 0)
		{
			return ParsePropertyLine(rawLine, equalsIndex, lineNumber, offset, length);
		}

		// Invalid line
		if (equalsIndex == 0)
		{
			return new InvalidLine(lineNumber, offset, length, rawLine, "Property line has empty key (starts with =)");
		}

		return new InvalidLine(lineNumber, offset, length, rawLine, "Line is not a valid section header, property, or comment");
	}

	/// <summary>
	/// Parses a section header line [section]
	/// </summary>
	private Line ParseSectionLine(string rawLine, string trimmedStart, int lineNumber, long offset, int length)
	{
		int closeBracket = trimmedStart.IndexOf(']');
		if (closeBracket == -1)
		{
			return new InvalidLine(lineNumber, offset, length, rawLine, "Section header missing closing bracket ]");
		}

		// Extract raw section name (between [ and ])
		string rawSectionName = trimmedStart.Substring(1, closeBracket - 1);

		// Normalize: trim whitespace inside brackets
		string sectionName = rawSectionName.Trim(' ', '\t');

		if (string.IsNullOrEmpty(sectionName))
		{
			return new InvalidLine(lineNumber, offset, length, rawLine, "Section name is empty");
		}

		// Extract trailing comment (anything after ] that looks like a comment)
		string trailing = trimmedStart.Substring(closeBracket + 1).TrimStart(' ', '\t');
		string trailingComment = string.Empty;

		if (!string.IsNullOrEmpty(trailing))
		{
			if (trailing[0] == ';' || trailing[0] == '#')
			{
				trailingComment = trailing.Substring(1).TrimStart(' ', '\t');
			}
			else if (!string.IsNullOrWhiteSpace(trailing))
			{
				// Non-comment text after ] - some dialects allow this, we'll accept but note
				_log.Info($"Line {lineNumber + 1}: Non-comment text after section closing bracket");
			}
		}

		return new SectionLine(lineNumber, offset, length, rawLine, rawSectionName, sectionName, trailingComment);
	}

	/// <summary>
	/// Parses a property line key=value
	/// </summary>
	private Line ParsePropertyLine(string rawLine, int equalsIndex, int lineNumber, long offset, int length)
	{
		string rawKey = rawLine.Substring(0, equalsIndex);
		string rawValue = rawLine.Substring(equalsIndex + 1);

		// Normalize key: trim leading and trailing whitespace
		string key = rawKey.Trim(' ', '\t');

		if (string.IsNullOrEmpty(key))
		{
			return new InvalidLine(lineNumber, offset, length, rawLine, "Property key is empty after normalization");
		}

		// Normalize value: trim leading whitespace only, preserve trailing
		string value = rawValue.TrimStart(' ', '\t');

		return new PropertyLine(lineNumber, offset, length, rawLine, rawKey, key, rawValue, value, '=');
	}

	/// <summary>
	/// Gets an existing section by name or creates a new one.
	/// </summary>
	private Section GetOrCreateSection(string name)
	{
		// Case-sensitive lookup for Core INI
		var existing = Sections.Find(s => s.Name == name);
		if (existing != null)
			return existing;

		var section = new Section(name);
		Sections.Add(section);
		return section;
	}

	/// <summary>
	/// Gets or creates the implicit global section (empty name).
	/// </summary>
	private Section GetOrCreateGlobalSection()
	{
		var existing = Sections.Find(s => s.IsGlobal);
		if (existing != null)
			return existing;

		var section = new Section(string.Empty);
		// Insert at beginning since global section comes first
		Sections.Insert(0, section);
		return section;
	}

	/// <summary>
	/// Detects the encoding of the stream by checking for BOM.
	/// </summary>
	private static Encoding DetectEncoding(Stream stream, out bool hasBom)
	{
		hasBom = false;
		if (stream.Length < 2)
			return Encoding.UTF8;

		byte[] bom = new byte[4];
		int read = stream.Read(bom, 0, Math.Min(4, (int)stream.Length));
		stream.Position = 0;

		// UTF-32 LE BOM
		if (read >= 4 && bom[0] == 0xFF && bom[1] == 0xFE && bom[2] == 0x00 && bom[3] == 0x00)
		{
			hasBom = true;
			return Encoding.UTF32;
		}

		// UTF-8 BOM
		if (read >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
		{
			hasBom = true;
			return Encoding.UTF8;
		}

		// UTF-16 LE BOM
		if (read >= 2 && bom[0] == 0xFF && bom[1] == 0xFE)
		{
			hasBom = true;
			return Encoding.Unicode;
		}

		// UTF-16 BE BOM
		if (read >= 2 && bom[0] == 0xFE && bom[1] == 0xFF)
		{
			hasBom = true;
			return Encoding.BigEndianUnicode;
		}

		// Default to UTF-8 without BOM
		return Encoding.UTF8;
	}

	/// <summary>
	/// Gets the BOM length in bytes for the given encoding.
	/// </summary>
	private static int GetBomLength(Encoding encoding)
	{
		if (encoding == Encoding.UTF8) return 3;
		if (encoding == Encoding.Unicode) return 2;
		if (encoding == Encoding.BigEndianUnicode) return 2;
		if (encoding == Encoding.UTF32) return 4;
		return 0;
	}
}