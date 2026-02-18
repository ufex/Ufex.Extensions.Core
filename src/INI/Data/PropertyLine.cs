namespace Ufex.Extensions.Core.INI.Data;

/// <summary>
/// A property (key=value) line.
/// Per Core INI spec: key is trimmed, value has leading whitespace after = ignored.
/// </summary>
public class PropertyLine : Line
{
	public override LineType Type => LineType.Property;

	/// <summary>
	/// The raw key as it appears before the delimiter (before normalization)
	/// </summary>
	public string RawKey { get; init; }

	/// <summary>
	/// The normalized key (trimmed of leading/trailing whitespace)
	/// </summary>
	public string Key { get; init; }

	/// <summary>
	/// The raw value as it appears after the delimiter (before normalization)
	/// </summary>
	public string RawValue { get; init; }

	/// <summary>
	/// The normalized value (leading whitespace after = ignored, trailing preserved)
	/// </summary>
	public string Value { get; init; }

	/// <summary>
	/// The delimiter character used ('=' for Core INI)
	/// </summary>
	public char Delimiter { get; init; }

	public PropertyLine(int lineNumber, long offset, int length, string rawText,
		string rawKey, string key, string rawValue, string value, char delimiter)
		: base(lineNumber, offset, length, rawText)
	{
		RawKey = rawKey;
		Key = key;
		RawValue = rawValue;
		Value = value;
		Delimiter = delimiter;
	}
}
