using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Ufex.API;
using Ufex.API.Validation;
using Ufex.Extensions.Core.OPC.Data;
using Ufex.Extensions.Core.ZIP.Data;

namespace Ufex.Extensions.Core.OPC;

/// <summary>
/// Validates OOXML (Office Open XML) specific requirements for Word/Excel/PowerPoint documents.
/// Extends OPC validation with document-type-specific checks.
/// </summary>
public class OoxmlValidator
{
	private readonly OpcPackage package;
	private readonly Stream fileStream;
	private readonly ValidationReport report;
	private readonly Logger log;
	private OoxmlDocumentType documentType;

	public OoxmlValidator(OpcPackage package, Stream fileStream, ValidationReport report, Logger log)
	{
		this.package = package;
		this.fileStream = fileStream;
		this.report = report;
		this.log = log;
		this.documentType = OoxmlDocumentType.Unknown;
	}

	/// <summary>
	/// Runs all OOXML validation checks.
	/// </summary>
	public void Validate()
	{
		log.Info("Starting OOXML validation");

		// Detect document type
		documentType = DetectDocumentType();

		// Document type specific checks
		ValidateRootPart();
		ValidateEntryOrder();

		// Summary
		report.Info($"V50: Document type detected: {documentType}");

		log.Info($"OOXML validation complete");
	}

	/// <summary>
	/// Gets the detected document type.
	/// </summary>
	public OoxmlDocumentType DocumentType => documentType;

	#region Document Type Detection

	/// <summary>
	/// Detects the OOXML document type by examining the package relationships.
	/// </summary>
	private OoxmlDocumentType DetectDocumentType()
	{
		// Find the officeDocument relationship from package relationships
		var officeDocRel = package.PackageRelationships?.Relationships
			.FirstOrDefault(r => r.Type.Equals(OpcConstants.OfficeDocumentRelationshipType, StringComparison.OrdinalIgnoreCase));

		if (officeDocRel == null)
		{
			log.Debug("No officeDocument relationship found - cannot determine OOXML document type");
			return OoxmlDocumentType.Unknown;
		}

		// Get the root part
		string rootPartName = officeDocRel.Target;
		if (!rootPartName.StartsWith("/"))
		{
			rootPartName = "/" + rootPartName;
		}

		var rootPart = package.FindPart(rootPartName);
		if (rootPart == null)
		{
			log.Debug($"Root part '{rootPartName}' not found");
			return OoxmlDocumentType.Unknown;
		}

		// Determine type based on content type
		string contentType = rootPart.ContentType.ToLowerInvariant();

		if (contentType.Contains("wordprocessingml") && contentType.Contains("document.main"))
		{
			return OoxmlDocumentType.WordprocessingML;
		}
		if (contentType.Contains("spreadsheetml") && contentType.Contains("sheet.main"))
		{
			return OoxmlDocumentType.SpreadsheetML;
		}
		if (contentType.Contains("presentationml") && contentType.Contains("presentation.main"))
		{
			return OoxmlDocumentType.PresentationML;
		}

		// Fallback: check part name
		string partNameLower = rootPartName.ToLowerInvariant();
		if (partNameLower.Contains("/word/document.xml"))
		{
			return OoxmlDocumentType.WordprocessingML;
		}
		if (partNameLower.Contains("/xl/workbook.xml"))
		{
			return OoxmlDocumentType.SpreadsheetML;
		}
		if (partNameLower.Contains("/ppt/presentation.xml"))
		{
			return OoxmlDocumentType.PresentationML;
		}

		return OoxmlDocumentType.Unknown;
	}

	#endregion

	#region Root Part Validation

	/// <summary>
	/// V30-V33: Root part validation.
	/// </summary>
	private void ValidateRootPart()
	{
		// Find the officeDocument relationship
		var officeDocRel = package.PackageRelationships?.Relationships
			.FirstOrDefault(r => r.Type.Equals(OpcConstants.OfficeDocumentRelationshipType, StringComparison.OrdinalIgnoreCase));

		// V30: Root part exists
		if (officeDocRel == null)
		{
			report.Error("V30: No officeDocument relationship found in package");
			return;
		}

		string rootPartName = officeDocRel.Target;
		if (!rootPartName.StartsWith("/"))
		{
			rootPartName = "/" + rootPartName;
		}

		var rootPart = package.FindPart(rootPartName);
		if (rootPart == null)
		{
			report.Error($"V30: Root part '{rootPartName}' does not exist");
			return;
		}

		// V31: Root part content type matches expected type
		string expectedContentType = GetExpectedRootPartContentType(documentType);
		if (!string.IsNullOrEmpty(expectedContentType) && 
			!rootPart.ContentType.Equals(expectedContentType, StringComparison.OrdinalIgnoreCase))
		{
			report.Error($"V31: Root part content type is '{rootPart.ContentType}', expected '{expectedContentType}'");
		}

		// V32: Root part is well-formed XML
		try
		{
			byte[] data = DecompressPartData(rootPart);
			string xmlContent = Encoding.UTF8.GetString(data);
			var doc = XDocument.Parse(xmlContent);

			// V33: Root element uses correct namespace
			string expectedNamespace = GetExpectedRootNamespace(documentType);
			if (!string.IsNullOrEmpty(expectedNamespace) && doc.Root != null)
			{
				if (!doc.Root.Name.NamespaceName.Equals(expectedNamespace, StringComparison.OrdinalIgnoreCase))
				{
					report.Warning($"V33: Root element namespace is '{doc.Root.Name.NamespaceName}', expected '{expectedNamespace}'");
				}
			}
		}
		catch (Exception ex)
		{
			report.Error($"V32: Root part is not well-formed XML: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets the expected content type for the root part based on document type.
	/// </summary>
	private string GetExpectedRootPartContentType(OoxmlDocumentType docType)
	{
		return docType switch
		{
			OoxmlDocumentType.WordprocessingML => "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml",
			OoxmlDocumentType.SpreadsheetML => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml",
			OoxmlDocumentType.PresentationML => "application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml",
			_ => string.Empty
		};
	}

	/// <summary>
	/// Gets the expected namespace for the root element based on document type.
	/// </summary>
	private string GetExpectedRootNamespace(OoxmlDocumentType docType)
	{
		return docType switch
		{
			OoxmlDocumentType.WordprocessingML => OpcConstants.WordprocessingMLNamespace,
			OoxmlDocumentType.SpreadsheetML => OpcConstants.SpreadsheetMLNamespace,
			OoxmlDocumentType.PresentationML => OpcConstants.PresentationMLNamespace,
			_ => string.Empty
		};
	}

	#endregion

	#region Entry Order Validation

	/// <summary>
	/// V34: Recommended entry order check.
	/// </summary>
	private void ValidateEntryOrder()
	{
		// The spec recommends [Content_Types].xml should be the first entry
		// We can't easily check this without access to the raw ZIP entry order,
		// so we'll just add an info message
		report.Info("V34: Entry order validation not implemented (requires ZIP entry order inspection)");
	}

	#endregion

	#region Helper Methods

	/// <summary>
	/// Decompresses part data.
	/// </summary>
	private byte[] DecompressPartData(OpcPart part)
	{
		var fileData = part.CompressedFile.FileData;
		fileStream.Position = fileData.StartPosition;

		byte[] compressedData = new byte[fileData.CompressedSize];
		fileStream.Read(compressedData, 0, compressedData.Length);

		if (fileData.CompressionMethod == 0)
		{
			return compressedData;
		}
		else if (fileData.CompressionMethod == 8)
		{
			using var compressedStream = new MemoryStream(compressedData);
			using var deflateStream = new System.IO.Compression.DeflateStream(compressedStream, System.IO.Compression.CompressionMode.Decompress);
			using var decompressedStream = new MemoryStream();
			deflateStream.CopyTo(decompressedStream);
			return decompressedStream.ToArray();
		}

		return compressedData;
	}

	#endregion
}
