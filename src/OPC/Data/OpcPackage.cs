namespace Ufex.Extensions.Core.OPC.Data;

/// <summary>
/// Top-level parsed OPC package model containing all OPC-layer data
/// </summary>
public class OpcPackage
{
	/// <summary>
	/// Parsed [Content_Types].xml (defaults + overrides)
	/// </summary>
	public ContentTypeEntry ContentTypes { get; init; } = new ContentTypeEntry();

	/// <summary>
	/// All relationship sets keyed by source part name (empty string for package-level relationships)
	/// </summary>
	public Dictionary<string, OpcRelationshipSet> Relationships { get; init; } = new Dictionary<string, OpcRelationshipSet>(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Parsed /docProps/core.xml (if present)
	/// </summary>
	public CoreProperties? CoreProperties { get; init; }

	/// <summary>
	/// List of all OPC parts (one per ZIP entry that is an OPC part)
	/// </summary>
	public List<OpcPart> Parts { get; init; } = new List<OpcPart>();

	/// <summary>
	/// Get package-level relationships (_rels/.rels)
	/// </summary>
	public OpcRelationshipSet? PackageRelationships => 
		Relationships.TryGetValue(string.Empty, out var rels) ? rels : null;

	/// <summary>
	/// Get relationships for a specific part
	/// </summary>
	/// <param name="partName">Part name (e.g., "/word/document.xml")</param>
	/// <returns>Relationship set or null if no relationships exist for this part</returns>
	public OpcRelationshipSet? GetRelationships(string partName)
	{
		return Relationships.TryGetValue(partName, out var rels) ? rels : null;
	}

	/// <summary>
	/// Find a part by name
	/// </summary>
	/// <param name="partName">Part name (e.g., "/word/document.xml")</param>
	/// <returns>OpcPart or null if not found</returns>
	public OpcPart? FindPart(string partName)
	{
		return Parts.FirstOrDefault(p => string.Equals(p.PartName, partName, StringComparison.OrdinalIgnoreCase));
	}
}
