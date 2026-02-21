namespace Ufex.Extensions.Core.PDF.Data;

/// <summary>
/// Represents the parsed PDF header (%PDF-x.y and optional binary marker)
/// </summary>
internal class PdfHeader
{
	/// <summary>
	/// Offset of the header in the file (typically 0)
	/// </summary>
	public long Offset { get; init; }

	/// <summary>
	/// Major version number
	/// </summary>
	public int MajorVersion { get; init; }

	/// <summary>
	/// Minor version number
	/// </summary>
	public int MinorVersion { get; init; }

	/// <summary>
	/// The raw header line (e.g., "%PDF-1.7")
	/// </summary>
	public string RawHeader { get; init; } = string.Empty;

	/// <summary>
	/// Whether the binary marker comment was present
	/// </summary>
	public bool HasBinaryMarker { get; init; }

	/// <summary>
	/// Byte length of the header section (header line + optional binary marker line)
	/// </summary>
	public long Length { get; init; }

	/// <summary>
	/// Version string (e.g., "1.7")
	/// </summary>
	public string VersionString => $"{MajorVersion}.{MinorVersion}";

	public override string ToString() => $"PDF {VersionString}";
}
