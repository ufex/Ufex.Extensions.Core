namespace Ufex.Extensions.Core.INI.Data;

/// <summary>
/// Line type classification per Core INI specification
/// </summary>
public enum LineType
{
	/// <summary>A line containing only whitespace</summary>
	Blank,
	/// <summary>A comment line starting with ; or #</summary>
	Comment,
	/// <summary>A section header line [section]</summary>
	Section,
	/// <summary>A property (key=value) line</summary>
	Property,
	/// <summary>An invalid line that doesn't conform to INI syntax</summary>
	Invalid
}

/// <summary>
/// Base class for all INI file lines.
/// Stores the raw line text and file position information.
/// </summary>
public abstract class Line
{
	/// <summary>
	/// The zero-based line number in the file
	/// </summary>
	public int LineNumber { get; init; }

	/// <summary>
	/// The byte offset where this line starts in the file
	/// </summary>
	public long Offset { get; init; }

	/// <summary>
	/// The length of this line in bytes (including line terminator)
	/// </summary>
	public int Length { get; init; }

	/// <summary>
	/// The raw text of the line (without line terminator)
	/// </summary>
	public string RawText { get; init; }

	/// <summary>
	/// The type classification of this line
	/// </summary>
	public abstract LineType Type { get; }

	protected Line(int lineNumber, long offset, int length, string rawText)
	{
		LineNumber = lineNumber;
		Offset = offset;
		Length = length;
		RawText = rawText;
	}
}
