namespace Ufex.Extensions.Core.OPC.Data;

/// <summary>
/// Represents the type of OOXML document (WordprocessingML, SpreadsheetML, or PresentationML).
/// </summary>
public enum OoxmlDocumentType
{
	/// <summary>
	/// Unknown or unrecognized OOXML document type.
	/// </summary>
	Unknown,

	/// <summary>
	/// WordprocessingML document (.docx).
	/// Root part: /word/document.xml
	/// Content type: application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml
	/// </summary>
	WordprocessingML,

	/// <summary>
	/// SpreadsheetML document (.xlsx).
	/// Root part: /xl/workbook.xml
	/// Content type: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml
	/// </summary>
	SpreadsheetML,

	/// <summary>
	/// PresentationML document (.pptx).
	/// Root part: /ppt/presentation.xml
	/// Content type: application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml
	/// </summary>
	PresentationML
}
