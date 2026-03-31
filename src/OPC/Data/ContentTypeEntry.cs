namespace Ufex.Extensions.Core.OPC.Data;

/// <summary>
/// Parsed [Content_Types].xml - maps part names and extensions to content types
/// </summary>
public class ContentTypeEntry
{
	/// <summary>
	/// Default content types by extension (extension → content type)
	/// </summary>
	public Dictionary<string, string> Defaults { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Override content types by part name (part name → content type)
	/// </summary>
	public Dictionary<string, string> Overrides { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Resolve the content type for a given part name using the OPC algorithm:
	/// 1. Check Overrides first (exact part name match)
	/// 2. Fall back to Defaults (by file extension)
	/// 3. Return null if no match found
	/// </summary>
	/// <param name="partName">Part name (e.g., "/word/document.xml")</param>
	/// <returns>Content type or null if not found</returns>
	public string? Resolve(string partName)
	{
		// Strip leading slash for consistency
		string normalizedName = partName.TrimStart('/');

		// Check Overrides first (with leading slash)
		if (Overrides.TryGetValue(partName, out string? overrideType))
		{
			return overrideType;
		}

		// Check Overrides without leading slash
		if (Overrides.TryGetValue(normalizedName, out overrideType))
		{
			return overrideType;
		}

		// Extract extension (everything after last '.')
		int lastDotIndex = partName.LastIndexOf('.');
		if (lastDotIndex >= 0 && lastDotIndex < partName.Length - 1)
		{
			string extension = partName.Substring(lastDotIndex + 1);
			if (Defaults.TryGetValue(extension, out string? defaultType))
			{
				return defaultType;
			}
		}

		return null;
	}
}
