namespace Ufex.Extensions.Core.OPC.Data;

/// <summary>
/// Represents a single relationship in an OPC package
/// </summary>
public class OpcRelationship
{
	/// <summary>
	/// Unique identifier for this relationship within its relationships part
	/// </summary>
	public string Id { get; init; } = string.Empty;

	/// <summary>
	/// Relationship type URI (e.g., officeDocument, styles, image)
	/// </summary>
	public string Type { get; init; } = string.Empty;

	/// <summary>
	/// Target URI (relative for internal parts, absolute for external resources)
	/// </summary>
	public string Target { get; init; } = string.Empty;

	/// <summary>
	/// Target mode: "Internal" (default) or "External"
	/// </summary>
	public string TargetMode { get; init; } = "Internal";

	/// <summary>
	/// True if this relationship points to an external resource
	/// </summary>
	public bool IsExternal => TargetMode == "External";
}
