namespace Ufex.Extensions.Core.OPC.Data;

/// <summary>
/// Namespace URIs, relationship types, and well-known part names for OPC packages
/// </summary>
public static class OpcConstants
{
	// Package namespaces
	public const string PackageCorePropertiesNamespace = "http://schemas.openxmlformats.org/package/2006/metadata/core-properties";
	public const string PackageContentTypesNamespace = "http://schemas.openxmlformats.org/package/2006/content-types";
	public const string PackageRelationshipsNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
	public const string OfficeDocumentRelationshipsNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
	public const string MarkupCompatibilityNamespace = "http://schemas.openxmlformats.org/markup-compatibility/2006";

	// Dublin Core namespaces
	public const string DublinCoreElementsNamespace = "http://purl.org/dc/elements/1.1/";
	public const string DublinCoreTermsNamespace = "http://purl.org/dc/terms/";

	// OOXML Core namespaces
	public const string WordprocessingMLNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
	public const string SpreadsheetMLNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
	public const string PresentationMLNamespace = "http://schemas.openxmlformats.org/presentationml/2006/main";
	public const string DrawingMLNamespace = "http://schemas.openxmlformats.org/drawingml/2006/main";

	// Relationship types
	public const string CorePropertiesRelationshipType = "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties";
	public const string ExtendedPropertiesRelationshipType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties";
	public const string CustomPropertiesRelationshipType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/custom-properties";
	public const string OfficeDocumentRelationshipType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument";
	public const string StylesRelationshipType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles";
	public const string ThemeRelationshipType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme";
	public const string ImageRelationshipType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image";
	public const string HyperlinkRelationshipType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink";

	// Content types
	public const string ContentTypesPartName = "[Content_Types].xml";
	public const string RelationshipsContentType = "application/vnd.openxmlformats-package.relationships+xml";
	public const string CorePropertiesContentType = "application/vnd.openxmlformats-package.core-properties+xml";

	// Well-known part names
	public const string CorePropertiesPartName = "/docProps/core.xml";
	public const string AppPropertiesPartName = "/docProps/app.xml";
	public const string CustomPropertiesPartName = "/docProps/custom.xml";
}
