namespace Ufex.FileTypes.INI.Data;

/// <summary>
/// Represents a property (key-value pair) in the INI data model.
/// </summary>
public class Property
{
	/// <summary>
	/// The normalized key name
	/// </summary>
	public string Key { get; init; }

	/// <summary>
	/// The normalized value
	/// </summary>
	public string Value { get; init; }

	/// <summary>
	/// The source line that defines this property
	/// </summary>
	public PropertyLine SourceLine { get; init; }

	/// <summary>
	/// The byte offset where this property starts in the file
	/// </summary>
	public long Offset => SourceLine.Offset;

	/// <summary>
	/// The byte length of this property line
	/// </summary>
	public int Length => SourceLine.Length;

	public Property(string key, string value, PropertyLine sourceLine)
	{
		Key = key;
		Value = value;
		SourceLine = sourceLine;
	}
}