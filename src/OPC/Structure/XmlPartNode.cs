using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Visual;
using Ufex.Extensions.Core.OPC.Data;

namespace Ufex.Extensions.Core.OPC.Structure;

/// <summary>
/// Represents an XML part node in the OPC package tree.
/// Extends PartNode to add deferred XML content loading and display.
/// The XML is decompressed and formatted only when the user clicks the node.
/// </summary>
public class XmlPartNode : PartNode
{
	public XmlPartNode(OpcPart part)
		: base(part, GetIconForPart(part))
	{
	}

	/// <summary>
	/// Overrides GetVisuals to provide deferred XML loading.
	/// Decompresses and formats the XML content on-demand when the user clicks the node.
	/// </summary>
	public override Ufex.API.Visual.Visual[] GetVisuals(IFileContext context)
	{
		// Build metadata table
		var metadataGrid = new DataGridVisual(BuildMetadataTable(), "Part Metadata");

		// Decompress and format XML
		string xmlContent;
		try
		{
			byte[] decompressedData = DecompressPartData(context.FileStream);
			xmlContent = Encoding.UTF8.GetString(decompressedData);

			// Try to pretty-print the XML
			try
			{
				var doc = XDocument.Parse(xmlContent);
				xmlContent = doc.ToString();
			}
			catch
			{
				// If parsing fails, just show the raw content
			}
		}
		catch (Exception ex)
		{
			xmlContent = $"Error loading XML content: {ex.Message}";
		}

		var textVisual = new TextVisual(xmlContent, "XML Content");

		return [ metadataGrid, textVisual ];
	}

	/// <summary>
	/// Decompresses the part data from the file stream.
	/// </summary>
	private byte[] DecompressPartData(FileStream fileStream)
	{
		var fileData = part.CompressedFile.FileData;
		var header = part.CompressedFile.Header;

		// Seek to the start of the compressed data
		fileStream.Position = fileData.StartPosition;

		// Read compressed data
		byte[] compressedData = new byte[fileData.CompressedSize];
		int bytesRead = fileStream.Read(compressedData, 0, compressedData.Length);
		if (bytesRead != compressedData.Length)
		{
			throw new Exception($"Failed to read compressed data. Expected {compressedData.Length} bytes, got {bytesRead}");
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
			throw new Exception($"Unsupported compression method: {fileData.CompressionMethod}");
		}
	}

	/// <summary>
	/// Gets the appropriate icon for the part based on its name/purpose.
	/// </summary>
	private static TreeViewIcon GetIconForPart(OpcPart part)
	{
		string fileName = part.FileName.ToLowerInvariant();

		// Use specific icons for well-known parts
		if (fileName.Contains("docprops/"))
		{
			return TreeViewIcon.Properties;
		}
		if (fileName == "[content_types].xml")
		{
			return TreeViewIcon.Gear;
		}

		// Default to Document icon for XML files
		return TreeViewIcon.Document;
	}
}
