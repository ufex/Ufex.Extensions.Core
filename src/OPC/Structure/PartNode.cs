using System;
using Ufex.API;
using Ufex.API.Format;
using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.Extensions.Core.OPC.Data;

namespace Ufex.Extensions.Core.OPC.Structure;

/// <summary>
/// Represents a generic OPC part node in the tree.
/// Displays a metadata table with part information when clicked.
/// </summary>
public class PartNode : TreeNode
{
	protected readonly OpcPart part;

	public PartNode(OpcPart part, TreeViewIcon icon)
		: base(part.FileName, icon, icon)
	{
		this.part = part;
	}

	public override Ufex.API.Visual.Visual[] Visuals
	{
		get
		{
			return [ new DataGridVisual(BuildMetadataTable(), "Part Metadata") ];
		}
	}

	/// <summary>
	/// Builds a metadata table showing part properties.
	/// </summary>
	protected virtual DynamicTableData BuildMetadataTable()
	{
		var td = new DynamicTableData(3, "OPC.PartMetadata");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");

		var header = part.CompressedFile.Header;
		var fileData = part.CompressedFile.FileData;

		td.AddRow("Part Name", part.PartName, "OPC part name");
		td.AddRow("File Name", part.FileName, "ZIP entry name");
		td.AddRow("Content Type", part.ContentType, "MIME type");
		td.AddRow("Compressed Size", fileData.CompressedSize, ByteCountFormatter.Format(fileData.CompressedSize));
		td.AddRow("Uncompressed Size", header.UncompressedSize, ByteCountFormatter.Format(header.UncompressedSize));
		td.AddRow("Compression Method", fileData.CompressionMethod, GetCompressionMethodName(fileData.CompressionMethod));
		td.AddRow("CRC-32", header.Crc32, $"0x{header.Crc32:X8}");
		td.AddRow("Last Modified Date", header.LastModFileDateText, "Date in DOS format");
		td.AddRow("Last Modified Time", header.LastModFileTimeText, "Time in DOS format");

		return td;
	}

	private static string GetCompressionMethodName(UInt16 method)
	{
		return method switch
		{
			0 => "Stored (no compression)",
			8 => "Deflate",
			_ => $"Unknown ({method})"
		};
	}
}
