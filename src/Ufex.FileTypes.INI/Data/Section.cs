namespace Ufex.FileTypes.INI.Data;

/// <summary>
/// Represents a section in the INI data model.
/// A section contains zero or more properties and tracks its source lines.
/// </summary>
public class Section
{
	/// <summary>
	/// The normalized section name (empty string for the global section)
	/// </summary>
	public string Name { get; init; }

	/// <summary>
	/// The list of properties in this section
	/// </summary>
	public List<Property> Properties { get; init; }

	/// <summary>
	/// All lines belonging to this section (header, properties, comments, blanks)
	/// </summary>
	public List<Line> Lines { get; init; }

	/// <summary>
	/// The section header line (null for the implicit global section)
	/// </summary>
	public SectionLine? HeaderLine { get; set; }

	/// <summary>
	/// Whether this is the implicit global section (properties before any [section] header)
	/// </summary>
	public bool IsGlobal => HeaderLine == null;

	/// <summary>
	/// The byte offset where this section starts in the file
	/// </summary>
	public long Offset => HeaderLine?.Offset ?? Lines.FirstOrDefault()?.Offset ?? 0;

	/// <summary>
	/// The total byte length of this section (from first to last line)
	/// </summary>
	public long Length
	{
		get
		{
			if (Lines.Count == 0) return 0;
			var lastLine = Lines[^1];
			return (lastLine.Offset + lastLine.Length) - Offset;
		}
	}

	public Section(string name)
	{
		Name = name;
		Properties = new List<Property>();
		Lines = new List<Line>();
	}

	/// <summary>
	/// Gets the effective value of a property by key (last occurrence wins per Core INI spec)
	/// </summary>
	public string? GetValue(string key, bool caseSensitive = true)
	{
		var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
		// Last occurrence wins per spec
		for (int i = Properties.Count - 1; i >= 0; i--)
		{
			if (comparer.Equals(Properties[i].Key, key))
				return Properties[i].Value;
		}
		return null;
	}
}