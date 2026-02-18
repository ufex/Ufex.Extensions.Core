namespace Ufex.Extensions.Core.INI.Data;

/// <summary>
/// A comment line starting with ; or #.
/// Per Core INI spec: comment delimiter and all following characters are ignored.
/// </summary>
public class CommentLine : Line
{
	public override LineType Type => LineType.Comment;

	/// <summary>
	/// The comment delimiter character (';' or '#')
	/// </summary>
	public char Delimiter { get; init; }

	/// <summary>
	/// The comment text after the delimiter (trimmed of leading whitespace)
	/// </summary>
	public string CommentText { get; init; }

	public CommentLine(int lineNumber, long offset, int length, string rawText, char delimiter, string commentText)
		: base(lineNumber, offset, length, rawText)
	{
		Delimiter = delimiter;
		CommentText = commentText;
	}
}
