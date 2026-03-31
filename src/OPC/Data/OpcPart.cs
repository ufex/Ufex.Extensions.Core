using Ufex.Extensions.Core.ZIP.Data;

namespace Ufex.Extensions.Core.OPC.Data;

/// <summary>
/// Individual OPC part metadata
/// </summary>
public class OpcPart
{
	/// <summary>
	/// Part name with leading '/' (e.g., "/word/document.xml")
	/// </summary>
	public string PartName { get; init; } = string.Empty;

	/// <summary>
	/// Content type resolved via [Content_Types].xml
	/// </summary>
	public string? ContentType { get; init; }

	/// <summary>
	/// Reference back to the ZIP CompressedFile for file offset/size data
	/// </summary>
	public CompressedFile CompressedFile { get; init; } = null!;

	/// <summary>
	/// True if this is an XML part (content type starts with text/ or application/ and contains +xml)
	/// </summary>
	public bool IsXml
	{
		get
		{
			if (string.IsNullOrEmpty(ContentType))
				return false;

			return (ContentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
			        ContentType.StartsWith("application/", StringComparison.OrdinalIgnoreCase)) &&
			       ContentType.Contains("+xml", StringComparison.OrdinalIgnoreCase);
		}
	}

	/// <summary>
	/// File name portion only (last segment after /)
	/// </summary>
	public string FileName
	{
		get
		{
			int lastSlash = PartName.LastIndexOf('/');
			return lastSlash >= 0 ? PartName.Substring(lastSlash + 1) : PartName;
		}
	}
}
