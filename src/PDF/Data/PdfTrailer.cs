namespace Ufex.Extensions.Core.PDF.Data;

/// <summary>
/// Represents the parsed PDF trailer section
/// </summary>
internal class PdfTrailer
{
	/// <summary>
	/// Offset of the trailer keyword or xref stream object
	/// </summary>
	public long Offset { get; init; }

	/// <summary>
	/// The trailer dictionary (or xref stream dictionary that serves as trailer)
	/// </summary>
	public PdfDictionary Dictionary { get; init; } = new();

	/// <summary>
	/// Byte offset of the cross-reference section (from startxref)
	/// </summary>
	public long StartXRefOffset { get; init; }

	/// <summary>
	/// Whether this trailer uses an xref stream (vs traditional xref table)
	/// </summary>
	public bool UsesXRefStream { get; init; }

	/// <summary>
	/// /Size - total number of entries in the xref table
	/// </summary>
	public int Size => (int)(Dictionary.GetInteger("Size") ?? 0);

	/// <summary>
	/// /Prev - offset of previous xref section (for incremental updates)
	/// </summary>
	public long? PrevOffset => Dictionary.GetInteger("Prev");

	/// <summary>
	/// Reference to the document catalog (/Root)
	/// </summary>
	public PdfReference? Root => Dictionary.GetReference("Root");

	/// <summary>
	/// Reference to the document info dictionary (/Info)
	/// </summary>
	public PdfReference? Info => Dictionary.GetReference("Info");

	/// <summary>
	/// Whether the PDF is encrypted (/Encrypt present)
	/// </summary>
	public bool IsEncrypted => Dictionary.ContainsKey("Encrypt");

	public override string ToString() => $"Trailer (Size={Size}, startxref={StartXRefOffset})";
}
