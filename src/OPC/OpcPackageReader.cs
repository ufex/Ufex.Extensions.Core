using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Ufex.API;
using Ufex.Extensions.Core.ZIP.Data;
using Ufex.Extensions.Core.OPC.Data;

namespace Ufex.Extensions.Core.OPC;

/// <summary>
/// Reads and parses the OPC (Open Packaging Conventions) layer from a ZIP archive.
/// This class extracts [Content_Types].xml, relationship files (.rels), and core properties
/// to build a complete OpcPackage model.
/// </summary>
public class OpcPackageReader
{
	private readonly List<CompressedFile> parts;
	private readonly Stream fileStream;
	private readonly Logger log;

	public OpcPackageReader(List<CompressedFile> parts, Stream fileStream, Logger log)
	{
		this.parts = parts;
		this.fileStream = fileStream;
		this.log = log;
	}

	/// <summary>
	/// Reads and parses the OPC package structure.
	/// </summary>
	/// <returns>A populated OpcPackage</returns>
	public OpcPackage Read()
	{
		log.Info("Starting OPC package parsing");

		// 1. Find and parse [Content_Types].xml
		var contentTypes = ParseContentTypes();

		// 2. Build OpcPart list with resolved content types
		var opcParts = BuildPartsList(contentTypes);

		// 3. Find and parse all .rels files
		var relationships = ParseRelationships();

		// 4. Find and parse docProps/core.xml
		var coreProperties = ParseCoreProperties();

		log.Info($"OPC parsing complete: {opcParts.Count} parts, {relationships.Count} relationship sets");

		return new OpcPackage
		{
			ContentTypes = contentTypes,
			Relationships = relationships,
			CoreProperties = coreProperties,
			Parts = opcParts
		};
	}

	/// <summary>
	/// Decompresses a CompressedFile entry and returns the raw bytes.
	/// </summary>
	private byte[] DecompressCompressedFile(CompressedFile file)
	{
		var header = file.Header;
		var fileData = file.FileData;

		// Seek to the start of the compressed data
		fileStream.Position = fileData.StartPosition;

		// Read the compressed data
		byte[] compressedData = new byte[fileData.CompressedSize];
		int bytesRead = fileStream.Read(compressedData, 0, compressedData.Length);
		if (bytesRead != compressedData.Length)
		{
			throw new Exception($"Failed to read compressed data for {header.FileNameText}. Expected {compressedData.Length} bytes, got {bytesRead}");
		}

		// Check compression method
		if (fileData.CompressionMethod == 0)
		{
			// Stored (no compression)
			return compressedData;
		}
		else if (fileData.CompressionMethod == 8)
		{
			// Deflate compression
			using var compressedStream = new MemoryStream(compressedData);
			using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
			using var decompressedStream = new MemoryStream();
			
			deflateStream.CopyTo(decompressedStream);
			return decompressedStream.ToArray();
		}
		else
		{
			throw new Exception($"Unsupported compression method {fileData.CompressionMethod} for {header.FileNameText}");
		}
	}

	/// <summary>
	/// Finds and parses [Content_Types].xml to build the ContentTypeEntry.
	/// </summary>
	private ContentTypeEntry ParseContentTypes()
	{
		log.Info("Parsing [Content_Types].xml");

		// Find [Content_Types].xml (must be at the root)
		var contentTypesFile = parts.OfType<CompressedFile>()
			.FirstOrDefault(f => f.Header.FileNameText.Equals("[Content_Types].xml", StringComparison.OrdinalIgnoreCase));

		if (contentTypesFile == null)
		{
			throw new Exception("Required [Content_Types].xml not found in package");
		}

		// Decompress and parse XML
		byte[] xmlBytes = DecompressCompressedFile(contentTypesFile);
		string xmlContent = Encoding.UTF8.GetString(xmlBytes);
		XDocument doc = XDocument.Parse(xmlContent);

		// Build ContentTypeEntry from XML
		var defaults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		var overrides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		XNamespace ns = OpcConstants.PackageCorePropertiesNamespace;
		var typesElement = doc.Root;

		if (typesElement != null)
		{
			// Parse Default elements (extension -> content type)
			foreach (var defaultElement in typesElement.Elements(typesElement.Name.Namespace + "Default"))
			{
				string? extension = defaultElement.Attribute("Extension")?.Value;
				string? contentType = defaultElement.Attribute("ContentType")?.Value;

				if (extension != null && contentType != null)
				{
					defaults[extension] = contentType;
					log.Debug($"  Default: .{extension} -> {contentType}");
				}
			}

			// Parse Override elements (part name -> content type)
			foreach (var overrideElement in typesElement.Elements(typesElement.Name.Namespace + "Override"))
			{
				string? partName = overrideElement.Attribute("PartName")?.Value;
				string? contentType = overrideElement.Attribute("ContentType")?.Value;

				if (partName != null && contentType != null)
				{
					overrides[partName] = contentType;
					log.Debug($"  Override: {partName} -> {contentType}");
				}
			}
		}

		log.Info($"Parsed {defaults.Count} default mappings, {overrides.Count} overrides");

		return new ContentTypeEntry
		{
			Defaults = defaults,
			Overrides = overrides
		};
	}

	/// <summary>
	/// Builds the list of OpcPart objects by resolving content types for all CompressedFile entries.
	/// </summary>
	private List<OpcPart> BuildPartsList(ContentTypeEntry contentTypes)
	{
		log.Info("Building OPC parts list");

		var opcParts = new List<OpcPart>();

		foreach (var part in parts.OfType<CompressedFile>())
		{
			string fileName = part.Header.FileNameText;

			// Skip [Content_Types].xml - it's package metadata, not a part
			if (fileName.Equals("[Content_Types].xml", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			// OPC part names must start with '/'
			string partName = fileName.StartsWith("/") ? fileName : "/" + fileName;

			// Resolve content type
			string? contentType = contentTypes.Resolve(partName);

			if (contentType == null)
			{
				log.Warn($"Could not resolve content type for part: {partName}");
				contentType = "application/octet-stream"; // fallback
			}

			opcParts.Add(new OpcPart
			{
				PartName = partName,
				ContentType = contentType,
				CompressedFile = part
			});

			log.Debug($"  Part: {partName} -> {contentType}");
		}

		log.Info($"Built {opcParts.Count} OPC parts");

		return opcParts;
	}

	/// <summary>
	/// Finds and parses all .rels files to build relationship sets.
	/// </summary>
	private Dictionary<string, OpcRelationshipSet> ParseRelationships()
	{
		log.Info("Parsing relationship files");

		var relationshipSets = new Dictionary<string, OpcRelationshipSet>(StringComparer.OrdinalIgnoreCase);

		// Find all .rels files
		var relsFiles = parts.OfType<CompressedFile>()
			.Where(f => f.Header.FileNameText.EndsWith(".rels", StringComparison.OrdinalIgnoreCase))
			.ToList();

		log.Info($"Found {relsFiles.Count} .rels files");

		foreach (var relsFile in relsFiles)
		{
			string fileName = relsFile.Header.FileNameText;
			log.Debug($"  Parsing {fileName}");

			// Decompress and parse XML
			byte[] xmlBytes = DecompressCompressedFile(relsFile);
			string xmlContent = Encoding.UTF8.GetString(xmlBytes);
			XDocument doc = XDocument.Parse(xmlContent);

			// Determine source part name
			// Package-level: _rels/.rels -> source is "/"
			// Part-level: word/_rels/document.xml.rels -> source is "/word/document.xml"
			string sourcePartName = GetSourcePartNameFromRelsFile(fileName);

			// Parse relationships
			var relationships = new List<OpcRelationship>();
			XNamespace ns = OpcConstants.PackageRelationshipsNamespace;
			var relationshipsElement = doc.Root;

			if (relationshipsElement != null)
			{
				foreach (var relElement in relationshipsElement.Elements(relationshipsElement.Name.Namespace + "Relationship"))
				{
					string? id = relElement.Attribute("Id")?.Value;
					string? type = relElement.Attribute("Type")?.Value;
					string? target = relElement.Attribute("Target")?.Value;
					string? targetMode = relElement.Attribute("TargetMode")?.Value;

					if (id != null && type != null && target != null)
					{
						relationships.Add(new OpcRelationship
						{
							Id = id,
							Type = type,
							Target = target,
							TargetMode = targetMode ?? "Internal"
						});

						log.Debug($"    Rel {id}: {type} -> {target}");
					}
				}
			}

			relationshipSets[sourcePartName] = new OpcRelationshipSet
			{
				SourcePartName = sourcePartName,
				Relationships = relationships
			};

			log.Debug($"  Parsed {relationships.Count} relationships for {sourcePartName}");
		}

		log.Info($"Parsed {relationshipSets.Count} relationship sets");

		return relationshipSets;
	}

	/// <summary>
	/// Derives the source part name from a .rels file path.
	/// </summary>
	private string GetSourcePartNameFromRelsFile(string relsFileName)
	{
		// Normalize to forward slashes and ensure leading slash
		string normalized = relsFileName.Replace('\\', '/');
		if (!normalized.StartsWith("/"))
		{
			normalized = "/" + normalized;
		}

		// Package-level: /_rels/.rels -> "/"
		if (normalized.Equals("/_rels/.rels", StringComparison.OrdinalIgnoreCase))
		{
			return "/";
		}

		// Part-level: /word/_rels/document.xml.rels -> /word/document.xml
		// Pattern: /path/_rels/filename.ext.rels -> /path/filename.ext
		if (normalized.Contains("/_rels/"))
		{
			// Remove "/_rels/" and ".rels" suffix
			string withoutRelsDir = normalized.Replace("/_rels/", "/");
			if (withoutRelsDir.EndsWith(".rels", StringComparison.OrdinalIgnoreCase))
			{
				return withoutRelsDir.Substring(0, withoutRelsDir.Length - 5);
			}
		}

		// Fallback (shouldn't happen in valid OPC packages)
		log.Warn($"Unexpected .rels file path format: {relsFileName}");
		return "/";
	}

	/// <summary>
	/// Finds and parses docProps/core.xml to build CoreProperties.
	/// </summary>
	private CoreProperties? ParseCoreProperties()
	{
		log.Info("Parsing core properties");

		// Find docProps/core.xml
		var coreFile = parts.OfType<CompressedFile>()
			.FirstOrDefault(f =>
			{
				string name = f.Header.FileNameText;
				return name.EndsWith("docProps/core.xml", StringComparison.OrdinalIgnoreCase) ||
					   name.EndsWith("docProps\\core.xml", StringComparison.OrdinalIgnoreCase);
			});

		if (coreFile == null)
		{
			log.Info("No core properties file found (optional)");
			return null;
		}

		// Decompress and parse XML
		byte[] xmlBytes = DecompressCompressedFile(coreFile);
		string xmlContent = Encoding.UTF8.GetString(xmlBytes);
		XDocument doc = XDocument.Parse(xmlContent);

		// Define namespaces
		XNamespace cp = OpcConstants.PackageCorePropertiesNamespace;
		XNamespace dc = OpcConstants.DublinCoreElementsNamespace;
		XNamespace dcterms = OpcConstants.DublinCoreTermsNamespace;

		var root = doc.Root;
		if (root == null)
		{
			return null;
		}

		// Helper to safely get element value
		string? GetValue(XNamespace ns, string localName)
		{
			return root.Element(ns + localName)?.Value;
		}

		// Helper to parse DateTime from W3CDTF format
		DateTime? ParseDateTime(XNamespace ns, string localName)
		{
			var value = GetValue(ns, localName);
			if (string.IsNullOrEmpty(value))
			{
				return null;
			}

			if (DateTime.TryParse(value, out DateTime result))
			{
				return result;
			}

			return null;
		}

		var coreProperties = new CoreProperties
		{
			Title = GetValue(dc, "title"),
			Creator = GetValue(dc, "creator"),
			Subject = GetValue(dc, "subject"),
			Keywords = GetValue(cp, "keywords"),
			Description = GetValue(dc, "description"),
			LastModifiedBy = GetValue(cp, "lastModifiedBy"),
			Revision = GetValue(cp, "revision"),
			Created = ParseDateTime(dcterms, "created"),
			Modified = ParseDateTime(dcterms, "modified"),
			Category = GetValue(cp, "category"),
			ContentStatus = GetValue(cp, "contentStatus"),
			Language = GetValue(dc, "language")
		};

		log.Info($"Parsed core properties: Title='{coreProperties.Title}', Creator='{coreProperties.Creator}'");

		return coreProperties;
	}
}
