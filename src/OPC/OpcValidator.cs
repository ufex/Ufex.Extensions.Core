using System;
using System.Collections.Generic;
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
/// Validates OPC (Open Packaging Conventions) compliance for a package.
/// Performs structural, relationship, and metadata validation according to ECMA-376 Part 2.
/// </summary>
public class OpcValidator
{
	private readonly OpcPackage package;
	private readonly List<CompressedFile> zipParts;
	private readonly Stream fileStream;
	private readonly ValidationReport report;
	private readonly Logger log;

	public OpcValidator(OpcPackage package, List<CompressedFile> zipParts, Stream fileStream, ValidationReport report, Logger log)
	{
		this.package = package;
		this.zipParts = zipParts;
		this.fileStream = fileStream;
		this.report = report;
		this.log = log;
	}

	/// <summary>
	/// Runs all OPC validation checks.
	/// </summary>
	public void Validate()
	{
		log.Info("Starting OPC validation");

		// Structural checks
		ValidateContentTypes();
		ValidateRelationships();
		ValidatePartNames();
		ValidateXmlParts();

		// Core properties checks
		ValidateCoreProperties();

		// Summary
		AddSummaryInfo();

		log.Info($"OPC validation complete: {report.NumErrors} error(s), {report.NumWarnings} warning(s)");
	}

	#region Structural Checks

	/// <summary>
	/// V01-V04: Content type validation.
	/// </summary>
	private void ValidateContentTypes()
	{
		// V01: [Content_Types].xml exists (already validated by OpcPackageReader - it throws if missing)
		// If we got here, [Content_Types].xml exists

		// V02: [Content_Types].xml is well-formed XML (already validated during parsing)
		// If we got here, it's well-formed

		// V03: Every part has a resolved content type
		foreach (var part in package.Parts)
		{
			if (string.IsNullOrEmpty(part.ContentType) || part.ContentType == "application/octet-stream")
			{
				report.Error($"V03: Part '{part.PartName}' does not have a resolved content type");
			}
		}

		// V04: No <Override> entries for non-existent parts
		var partNames = new HashSet<string>(package.Parts.Select(p => p.PartName), StringComparer.OrdinalIgnoreCase);
		foreach (var overrideName in package.ContentTypes.Overrides.Keys)
		{
			if (!partNames.Contains(overrideName))
			{
				report.Warning($"V04: Content type override exists for non-existent part '{overrideName}'");
			}
		}
	}

	/// <summary>
	/// V05-V08: Relationship validation.
	/// </summary>
	private void ValidateRelationships()
	{
		// V05: _rels/.rels exists if package has relationships
		if (package.PackageRelationships != null && package.PackageRelationships.Relationships.Count > 0)
		{
			var packageRelsFile = zipParts.OfType<CompressedFile>()
				.FirstOrDefault(f => f.Header.FileNameText.Equals("_rels/.rels", StringComparison.OrdinalIgnoreCase) ||
									 f.Header.FileNameText.Equals("_rels\\.rels", StringComparison.OrdinalIgnoreCase));
			if (packageRelsFile == null)
			{
				report.Warning("V05: Package has relationships but _rels/.rels file is missing");
			}
		}

		// V06: All .rels files are well-formed XML (already validated during parsing)
		// If we got here, they're well-formed

		// V07: Relationship targets exist for TargetMode="Internal"
		foreach (var relSet in package.Relationships.Values)
		{
			foreach (var rel in relSet.Relationships)
			{
				if (!rel.IsExternal)
				{
					// Resolve target relative to source
					string targetPartName = ResolveRelativePartName(relSet.SourcePartName, rel.Target);
					var targetPart = package.FindPart(targetPartName);
					
					if (targetPart == null)
					{
						report.Warning($"V07: Internal relationship target '{targetPartName}' does not exist (from {relSet.SourcePartName}, rel {rel.Id})");
					}
				}
			}
		}

		// V08: No duplicate relationship IDs within a single .rels
		foreach (var relSet in package.Relationships.Values)
		{
			var idCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			foreach (var rel in relSet.Relationships)
			{
				if (idCount.ContainsKey(rel.Id))
				{
					idCount[rel.Id]++;
				}
				else
				{
					idCount[rel.Id] = 1;
				}
			}

			foreach (var kvp in idCount.Where(kvp => kvp.Value > 1))
			{
				report.Warning($"V08: Duplicate relationship ID '{kvp.Key}' in relationship set for '{relSet.SourcePartName}' ({kvp.Value} occurrences)");
			}
		}
	}

	/// <summary>
	/// V09-V10: Part name validation.
	/// </summary>
	private void ValidatePartNames()
	{
		// V09: Part names don't contain . or .. segments
		foreach (var part in package.Parts)
		{
			var segments = part.PartName.Split('/');
			foreach (var segment in segments)
			{
				if (segment == "." || segment == "..")
				{
					report.Error($"V09: Part name '{part.PartName}' contains invalid segment '{segment}'");
				}
			}
		}

		// V10: No duplicate part names (case-insensitive)
		var nameCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		foreach (var part in package.Parts)
		{
			if (nameCount.ContainsKey(part.PartName))
			{
				nameCount[part.PartName]++;
			}
			else
			{
				nameCount[part.PartName] = 1;
			}
		}

		foreach (var kvp in nameCount.Where(kvp => kvp.Value > 1))
		{
			report.Warning($"V10: Duplicate part name '{kvp.Key}' ({kvp.Value} occurrences)");
		}
	}

	/// <summary>
	/// V11, V40-V41: XML part validation.
	/// </summary>
	private void ValidateXmlParts()
	{
		foreach (var part in package.Parts.Where(p => p.IsXml))
		{
			try
			{
				byte[] data = DecompressPartData(part);

				// V40: Check encoding (UTF-8 or UTF-16)
				var encoding = DetectEncoding(data);
				if (encoding != null && encoding.CodePage != Encoding.UTF8.CodePage && encoding.CodePage != Encoding.Unicode.CodePage)
				{
					report.Warning($"V40: XML part '{part.PartName}' uses non-standard encoding '{encoding.EncodingName}' (should be UTF-8 or UTF-16)");
				}

				// V41: No BOM in UTF-8 XML parts
				if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
				{
					report.Warning($"V41: XML part '{part.PartName}' has UTF-8 BOM (should be omitted per OPC spec)");
				}

				// V11: XML is well-formed
				string xmlContent = Encoding.UTF8.GetString(data);
				XDocument.Parse(xmlContent);
			}
			catch (Exception ex)
			{
				report.Error($"V11: XML part '{part.PartName}' is not well-formed: {ex.Message}");
			}
		}
	}

	#endregion

	#region Core Properties Checks

	/// <summary>
	/// V20-V25: Core properties validation.
	/// </summary>
	private void ValidateCoreProperties()
	{
		// V21: At most one Core Properties part
		var corePropsRels = package.PackageRelationships?.Relationships
			.Where(r => r.Type.Equals(OpcConstants.CorePropertiesRelationshipType, StringComparison.OrdinalIgnoreCase))
			.ToList();

		if (corePropsRels != null && corePropsRels.Count > 1)
		{
			report.Error($"V21: Package has {corePropsRels.Count} Core Properties relationships (should be at most one)");
		}

		// V20: Core Properties part exists (SHOULD)
		if (package.CoreProperties == null)
		{
			report.Info("V20: Core Properties part not found (recommended but optional)");
			return;
		}

		var props = package.CoreProperties;

		// V22: Has dc:creator
		if (string.IsNullOrEmpty(props.Creator))
		{
			report.Info("V22: Core Properties lacks dc:creator (recommended)");
		}

		// V23: Has dcterms:created
		if (!props.Created.HasValue)
		{
			report.Info("V23: Core Properties lacks dcterms:created (recommended)");
		}

		// V24: Has dcterms:modified
		if (!props.Modified.HasValue)
		{
			report.Info("V24: Core Properties lacks dcterms:modified (recommended)");
		}

		// V25: Valid W3CDTF timestamps (basic check - DateTime.TryParse already validated these)
		// If we got here, they're valid
	}

	#endregion

	#region Summary

	/// <summary>
	/// V51-V53: Summary information.
	/// </summary>
	private void AddSummaryInfo()
	{
		// V51: Total part count
		report.Info($"V51: Package contains {package.Parts.Count} part(s)");

		// V52: Total relationship count
		int totalRels = package.Relationships.Values.Sum(rs => rs.Relationships.Count);
		report.Info($"V52: Package contains {totalRels} relationship(s) across {package.Relationships.Count} relationship set(s)");

		// V53: Content type distribution
		var contentTypeCounts = package.Parts
			.GroupBy(p => p.ContentType)
			.OrderByDescending(g => g.Count())
			.Take(10);

		report.Info("V53: Content type distribution (top 10):");
		foreach (var group in contentTypeCounts)
		{
			report.Info($"  {group.Key}: {group.Count()} part(s)");
		}
	}

	#endregion

	#region Helper Methods

	/// <summary>
	/// Resolves a relative target URI to an absolute part name.
	/// </summary>
	private string ResolveRelativePartName(string sourcePartName, string target)
	{
		// If target is absolute (starts with /), return it
		if (target.StartsWith("/"))
		{
			return target;
		}

		// Otherwise, resolve relative to source directory
		string sourceDir = sourcePartName == "/" ? "/" : System.IO.Path.GetDirectoryName(sourcePartName.Replace('/', '\\'))?.Replace('\\', '/') ?? "/";
		if (!sourceDir.StartsWith("/"))
		{
			sourceDir = "/" + sourceDir;
		}
		if (!sourceDir.EndsWith("/"))
		{
			sourceDir += "/";
		}

		return sourceDir + target;
	}

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

	/// <summary>
	/// Detects the encoding of a byte array.
	/// </summary>
	private Encoding? DetectEncoding(byte[] data)
	{
		if (data.Length < 2)
		{
			return null;
		}

		// Check for BOM
		if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
		{
			return Encoding.UTF8;
		}
		if (data[0] == 0xFF && data[1] == 0xFE)
		{
			return Encoding.Unicode; // UTF-16 LE
		}
		if (data[0] == 0xFE && data[1] == 0xFF)
		{
			return Encoding.BigEndianUnicode; // UTF-16 BE
		}

		// Default to UTF-8
		return Encoding.UTF8;
	}

	#endregion
}
