using System;
using System.IO;
using System.IO.Compression;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Visual;
using Ufex.Extensions.Core.OPC.Data;

namespace Ufex.Extensions.Core.OPC.Structure;

/// <summary>
/// Represents an image part node in the OPC package tree.
/// Extends PartNode to add deferred image content loading and display.
/// The image is decompressed only when the user clicks the node.
/// </summary>
public class ImagePartNode : PartNode
{
	public ImagePartNode(OpcPart part)
		: base(part, TreeViewIcon.Image)
	{
	}

	/// <summary>
	/// Overrides GetVisuals to provide deferred image loading.
	/// Decompresses and displays the image content on-demand when the user clicks the node.
	/// </summary>
	public override Ufex.API.Visual.Visual[] GetVisuals(IFileContext context)
	{
		// Build metadata table
		var metadataGrid = new DataGridVisual(BuildMetadataTable(), "Part Metadata");

		// Try to load image
		try
		{
			byte[] imageData = DecompressPartData(context.FileStream);
			var imageStream = new MemoryStream(imageData);
			var imageVisual = new RasterImageVisual(imageStream, "Image Preview");

			return [ metadataGrid, imageVisual ];
		}
		catch (Exception ex)
		{
			// If image loading fails, just show the metadata
			var errorVisual = new TextVisual($"Error loading image: {ex.Message}", "Error");
			return [ metadataGrid, errorVisual ];
		}
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
}
