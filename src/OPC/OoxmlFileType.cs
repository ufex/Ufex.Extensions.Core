using System;
using System.Linq;
using Ufex.Extensions.Core.ZIP;
using Ufex.Extensions.Core.ZIP.Data;
using Ufex.Extensions.Core.OPC.Data;

namespace Ufex.Extensions.Core.OPC;

/// <summary>
/// FileType implementation for OOXML (Office Open XML) documents.
/// Thin subclass of OpcFileType that adds:
/// - Document type detection (Word/Excel/PowerPoint)
/// - OOXML-specific validation (root part, namespaces, etc.)
/// - Document type in QuickInfo
/// </summary>
public class OoxmlFileType : OpcFileType
{
	protected OoxmlDocumentType DocumentType { get; set; }

	public OoxmlFileType()
	{
		DocumentType = OoxmlDocumentType.Unknown;
	}

	public override bool ProcessFile()
	{
		// First, do the OPC-level parsing (which also does ZIP parsing)
		ZipStreamReader zipReader = new ZipStreamReader(FileInStream, Log, ValidationReport);
		bool result = zipReader.Read();

		if (!result)
		{
			return false;
		}

		// Parse the OPC layer
		try
		{
			var opcReader = new OpcPackageReader(
				zipReader.Parts.OfType<CompressedFile>().ToList(),
				FileInStream,
				Log
			);
			Package = opcReader.Read();

			// Build QuickInfo (ZIP + OPC + OOXML)
			BuildQuickInfo(zipReader);
			BuildOpcQuickInfo();
			BuildOoxmlQuickInfo();

			// Build Visuals (inherit ZIP FileMap)
			BuildVisuals(zipReader);

			// Build Structure (OPC-specific tree)
			BuildOpcStructure();

			// Run OPC validation
			var opcValidator = new OpcValidator(
				Package,
				zipReader.Parts.OfType<CompressedFile>().ToList(),
				FileInStream,
				ValidationReport,
				Log
			);
			opcValidator.Validate();

			// Run OOXML validation
			var ooxmlValidator = new OoxmlValidator(
				Package,
				FileInStream,
				ValidationReport,
				Log
			);
			ooxmlValidator.Validate();
			DocumentType = ooxmlValidator.DocumentType;

			return true;
		}
		catch (Exception ex)
		{
			ValidationReport.Error($"Failed to parse OOXML document: {ex.Message}");
			Log.Error($"OOXML parsing failed: {ex}");
			return false;
		}
	}

	/// <summary>
	/// Adds OOXML-specific metadata to the QuickInfo table.
	/// </summary>
	protected virtual void BuildOoxmlQuickInfo()
	{
		if (Package == null)
		{
			return;
		}

		var quickInfo = QuickInfoTable;

		// Add document type
		string docTypeText = DocumentType switch
		{
			OoxmlDocumentType.WordprocessingML => "Word Document (.docx)",
			OoxmlDocumentType.SpreadsheetML => "Excel Spreadsheet (.xlsx)",
			OoxmlDocumentType.PresentationML => "PowerPoint Presentation (.pptx)",
			_ => "Unknown"
		};

		quickInfo.AddRow("Document Type", docTypeText);
	}
}
