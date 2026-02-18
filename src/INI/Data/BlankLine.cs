namespace Ufex.Extensions.Core.INI.Data;

/// <summary>
/// A blank line containing only whitespace.
/// Per Core INI spec: blank lines have no effect on the data model.
/// </summary>
public class BlankLine : Line
{
	public override LineType Type => LineType.Blank;

	public BlankLine(int lineNumber, long offset, int length, string rawText)
		: base(lineNumber, offset, length, rawText)
	{
	}
}
