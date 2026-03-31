namespace Ufex.Extensions.Core.OPC.Data;

/// <summary>
/// Represents the set of relationships for a single source (package or part)
/// </summary>
public class OpcRelationshipSet
{
	/// <summary>
	/// Source part name (e.g., "/word/document.xml") or empty string for package-level relationships
	/// </summary>
	public string SourcePartName { get; init; } = string.Empty;

	/// <summary>
	/// List of relationships from this source
	/// </summary>
	public List<OpcRelationship> Relationships { get; init; } = new List<OpcRelationship>();

	/// <summary>
	/// True if this is the package-level relationships (_rels/.rels)
	/// </summary>
	public bool IsPackageLevel => string.IsNullOrEmpty(SourcePartName);
}
