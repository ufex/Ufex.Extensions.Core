namespace Ufex.Extensions.Core.INI.Data;

/// <summary>
/// An invalid line that doesn't conform to Core INI syntax.
/// Examples: property line without =, section without closing ], etc.
/// </summary>
public class InvalidLine : Line
{
	public override LineType Type => LineType.Invalid;

	/// <summary>
	/// A description of why this line is invalid
	/// </summary>
	public string Reason { get; init; }

	public InvalidLine(int lineNumber, long offset, int length, string rawText, string reason)
		: base(lineNumber, offset, length, rawText)
	{
		Reason = reason;
	}
}
