using System;
using System.Collections.Generic;
using System.Linq;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.Extensions.Core.ZIP;
using Ufex.Extensions.Core.ZIP.Data;
using Ufex.Extensions.Core.OPC.Data;
using Ufex.Extensions.Core.OPC.Structure;

namespace Ufex.Extensions.Core.OPC;

/// <summary>
/// FileType implementation for OPC (Open Packaging Conventions) packages.
/// Base class for OOXML and other OPC-based file formats.
/// Inherits ZIP parsing from ZipFileType and adds OPC-specific features:
/// - Content type resolution
/// - Relationship parsing
/// - Core properties metadata
/// - Folder-based tree view with deferred XML loading
/// </summary>
public class OpcFileType : ZipFileType
{
	protected OpcPackage? Package { get; set; }

	public OpcFileType()
	{
		// Tabs are already enabled by ZipFileType
	}

	public override bool ProcessFile()
	{
		// First, do the ZIP-level parsing
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

			// Build QuickInfo (ZIP + OPC)
			BuildQuickInfo(zipReader);
			BuildOpcQuickInfo();

			// Build Visuals (inherit ZIP FileMap)
			BuildVisuals(zipReader);

			// Build Structure (OPC-specific tree)
			BuildOpcStructure();

			// Run OPC validation
			var validator = new OpcValidator(
				Package,
				zipReader.Parts.OfType<CompressedFile>().ToList(),
				FileInStream,
				ValidationReport,
				Log
			);
			validator.Validate();

			return true;
		}
		catch (Exception ex)
		{
			ValidationReport.Error($"Failed to parse OPC package: {ex.Message}");
			Log.Error($"OPC parsing failed: {ex}");
			return false;
		}
	}

	/// <summary>
	/// Adds OPC-specific metadata to the QuickInfo table.
	/// </summary>
	protected virtual void BuildOpcQuickInfo()
	{
		if (Package == null)
		{
			return;
		}

		var quickInfo = QuickInfoTable;

		// Core properties
		if (Package.CoreProperties != null)
		{
			var props = Package.CoreProperties;

			if (!string.IsNullOrEmpty(props.Title))
			{
				quickInfo.AddRow("Title", props.Title);
			}
			if (!string.IsNullOrEmpty(props.Creator))
			{
				quickInfo.AddRow("Author", props.Creator);
			}
			if (!string.IsNullOrEmpty(props.LastModifiedBy))
			{
				quickInfo.AddRow("Last Modified By", props.LastModifiedBy);
			}
			if (props.Created.HasValue)
			{
				quickInfo.AddRow("Created", props.Created.Value.ToString("yyyy-MM-dd HH:mm:ss"));
			}
			if (props.Modified.HasValue)
			{
				quickInfo.AddRow("Modified", props.Modified.Value.ToString("yyyy-MM-dd HH:mm:ss"));
			}
			if (!string.IsNullOrEmpty(props.Subject))
			{
				quickInfo.AddRow("Subject", props.Subject);
			}
			if (!string.IsNullOrEmpty(props.Keywords))
			{
				quickInfo.AddRow("Keywords", props.Keywords);
			}
		}

		// OPC statistics
		quickInfo.AddRow("Number of Parts", Package.Parts.Count.ToString());
		int totalRels = Package.Relationships.Values.Sum(rs => rs.Relationships.Count);
		quickInfo.AddRow("Number of Relationships", totalRels.ToString());
	}

	/// <summary>
	/// Builds the OPC-specific tree structure with folders and parts.
	/// </summary>
	protected virtual void BuildOpcStructure()
	{
		if (Package == null)
		{
			return;
		}

		// Clear any existing tree nodes
		TreeNodes.Clear();

		// Build a hierarchical folder structure
		var folderMap = new Dictionary<string, PartFolderNode>(StringComparer.OrdinalIgnoreCase);

		// Add all parts to the tree
		foreach (var part in Package.Parts.OrderBy(p => p.PartName))
		{
			// Split part name into folder segments
			// e.g., "/word/document.xml" -> ["word"], filename = "document.xml"
			string partName = part.PartName.TrimStart('/');
			string[] segments = partName.Split('/');

			TreeNode parentNode;
			if (segments.Length == 1)
			{
				// Part is at root level
				parentNode = null;
			}
			else
			{
				// Part is in a folder
				string currentPath = "";
				TreeNode currentParent = null;

				for (int i = 0; i < segments.Length - 1; i++)
				{
					currentPath += "/" + segments[i];

					if (!folderMap.ContainsKey(currentPath))
					{
						var folderNode = new PartFolderNode(segments[i]);
						folderMap[currentPath] = folderNode;

						if (currentParent == null)
						{
							// Add to root
							TreeNodes.Add(folderNode);
						}
						else
						{
							// Add to parent folder
							currentParent.Nodes.Add(folderNode);
						}
					}

					currentParent = folderMap[currentPath];
				}

				parentNode = currentParent;
			}

			// Create appropriate node type based on content type
			TreeNode partNode = CreatePartNode(part);

			if (parentNode == null)
			{
				TreeNodes.Add(partNode);
			}
			else
			{
				parentNode.Nodes.Add(partNode);
			}
		}
	}

	/// <summary>
	/// Creates the appropriate TreeNode subclass for a part based on its content type.
	/// </summary>
	protected virtual TreeNode CreatePartNode(OpcPart part)
	{
		// Check if it's an XML part
		if (part.IsXml)
		{
			return new XmlPartNode(part);
		}

		// Check if it's an image part
		string contentType = part.ContentType.ToLowerInvariant();
		if (contentType.Contains("image/"))
		{
			return new ImagePartNode(part);
		}

		// Default to generic part node
		return new PartNode(part, TreeViewIcon.Document);
	}
}
