namespace Ufex.Extensions.Core.OPC.Data;

/// <summary>
/// Parsed Core Properties from /docProps/core.xml (Dublin Core metadata)
/// </summary>
public class CoreProperties
{
	/// <summary>
	/// Document title (dc:title)
	/// </summary>
	public string? Title { get; init; }

	/// <summary>
	/// Primary author/creator (dc:creator)
	/// </summary>
	public string? Creator { get; init; }

	/// <summary>
	/// Subject matter (dc:subject)
	/// </summary>
	public string? Subject { get; init; }

	/// <summary>
	/// Space-separated keywords (cp:keywords)
	/// </summary>
	public string? Keywords { get; init; }

	/// <summary>
	/// Abstract or summary (dc:description)
	/// </summary>
	public string? Description { get; init; }

	/// <summary>
	/// User who last saved the document (cp:lastModifiedBy)
	/// </summary>
	public string? LastModifiedBy { get; init; }

	/// <summary>
	/// Revision number (cp:revision)
	/// </summary>
	public string? Revision { get; init; }

	/// <summary>
	/// Creation timestamp UTC (dcterms:created)
	/// </summary>
	public DateTime? Created { get; init; }

	/// <summary>
	/// Last modification timestamp UTC (dcterms:modified)
	/// </summary>
	public DateTime? Modified { get; init; }

	/// <summary>
	/// Application-defined category (cp:category)
	/// </summary>
	public string? Category { get; init; }

	/// <summary>
	/// Content status (e.g., Draft, Final) (cp:contentStatus)
	/// </summary>
	public string? ContentStatus { get; init; }

	/// <summary>
	/// Primary language BCP 47 tag (dc:language)
	/// </summary>
	public string? Language { get; init; }
}
