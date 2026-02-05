namespace Ufex.FileTypes.INI.Data;

/// <summary>
/// A section header line [section].
/// Per Core INI spec: section name is trimmed of leading/trailing whitespace inside brackets.
/// </summary>
public class SectionLine : Line
{
	public override LineType Type => LineType.Section;

	/// <summary>
	/// The raw section name as it appears between the brackets (before normalization)
	/// </summary>
	public string RawSectionName { get; init; }

	/// <summary>
	/// The normalized section name (trimmed of leading/trailing whitespace)
	/// </summary>
	public string SectionName { get; init; }

	/// <summary>
	/// Any trailing comment text after the closing bracket (may be empty)
	/// </summary>
	public string TrailingComment { get; init; }

	public SectionLine(int lineNumber, long offset, int length, string rawText,
		string rawSectionName, string sectionName, string trailingComment)
		: base(lineNumber, offset, length, rawText)
	{
		RawSectionName = rawSectionName;
		SectionName = sectionName;
		TrailingComment = trailingComment;
	}
}
