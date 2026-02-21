using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.Extensions.Core.PDF.Data;
using Ufex.Extensions.Core.PDF.Structure;

namespace Ufex.Extensions.Core.PDF;

/// <summary>
/// PDF FileType module for Ufex.
/// Parses PDF files and populates structure, visual, and quick info data.
/// </summary>
public class PdfFileType : FileType
{
	public PdfFileType()
	{
		ShowTechnical = true;
		ShowGraphic = false;
		ShowFileCheck = true;
	}

	public override bool ProcessFile()
	{
		// Read entire file into byte array for random-access parsing
		FileInStream.Position = 0;
		byte[] data = new byte[FileInStream.Length];
		int bytesRead = 0;
		while (bytesRead < data.Length)
		{
			int read = FileInStream.Read(data, bytesRead, data.Length - bytesRead);
			if (read == 0) break;
			bytesRead += read;
		}

		var reader = new PdfStreamReader(data, Logger, ValidationReport);
		bool result = reader.Read();

		if (!result)
		{
			Logger.LogError("Failed to parse PDF file");
			return false;
		}

		BuildQuickInfo(reader);
		BuildVisuals(reader);
		BuildStructure(reader);

		return true;
	}

	private void BuildQuickInfo(PdfStreamReader reader)
	{
		var qi = QuickInfoTable;

		qi.AddRow("PDF Version", reader.Header.VersionString);
		qi.AddRow("File Size", Ufex.API.Format.ByteCountFormatter.Format((ulong)reader.FileSize));
		qi.AddRow("Pages", reader.PageCount.ToString());
		qi.AddRow("Objects", reader.Objects.Count.ToString());
		qi.AddRow("XRef Type", reader.UsesXRefStream ? "XRef Stream" : "XRef Table");
		qi.AddRow("XRef Entries", reader.XRefEntries.Count.ToString());

		if (reader.Trailer.IsEncrypted)
			qi.AddRow("Encrypted", "Yes");

		// Metadata from Info dictionary
		string[] metaKeys = ["Title", "Author", "Subject", "Creator", "Producer", "Keywords", "CreationDate", "ModDate"];
		foreach (var key in metaKeys)
		{
			if (reader.Metadata.TryGetValue(key, out string? value))
				qi.AddRow(FormatMetaKey(key), value);
		}
	}

	private void BuildVisuals(PdfStreamReader reader)
	{
		var spans = new List<FileSpan>();

		// Header span
		spans.Add(new FileSpan(reader.Header.Offset, reader.Header.Offset + reader.Header.Length,
			"PDF Header", 0xFF4488CC));

		// Object spans
		foreach (var obj in reader.Objects.Values.OrderBy(o => o.Offset))
		{
			uint color = GetObjectColor(obj);
			spans.Add(new FileSpan(obj.Offset, obj.EndOffset,
				$"Object {obj.ObjectNumber} ({obj.DisplayName})", color));
		}

		// XRef span
		if (reader.XRefOffset > 0)
		{
			spans.Add(new FileSpan(reader.XRefOffset, reader.Trailer.Offset > reader.XRefOffset
				? reader.Trailer.Offset : reader.XRefOffset + 100,
				"Cross-Reference Table", 0xFFCC8844));
		}

		var map = new FileMap(spans.ToArray(), (ulong)reader.FileSize);
		VisualsList.Add(map);
	}

	private void BuildStructure(PdfStreamReader reader)
	{
		// Header node
		TreeNodes.Add(new HeaderNode(reader.Header));

		// Group objects by type for organized tree display
		var catalog = new List<IndirectObject>();
		var pages = new List<IndirectObject>();
		var fonts = new List<IndirectObject>();
		var images = new List<IndirectObject>();
		var streams = new List<IndirectObject>();
		var other = new List<IndirectObject>();

		foreach (var obj in reader.Objects.Values.OrderBy(o => o.ObjectNumber))
		{
			switch (obj.TypeName)
			{
				case "Catalog":
					catalog.Add(obj);
					break;
				case "Pages":
				case "Page":
					pages.Add(obj);
					break;
				case "Font":
				case "FontDescriptor":
					fonts.Add(obj);
					break;
				case "XObject" when obj.SubtypeName == "Image":
					images.Add(obj);
					break;
				case "XRef":
				case "ObjStm":
					streams.Add(obj);
					break;
				default:
					if (obj.Value is PdfStream)
						streams.Add(obj);
					else
						other.Add(obj);
					break;
			}
		}

		// Add objects grouped into folders
		AddObjectGroup("Document", catalog, Ufex.API.Tree.TreeViewIcon.Document);
		AddObjectGroup("Pages", pages, Ufex.API.Tree.TreeViewIcon.FolderOpen);
		AddObjectGroup("Fonts", fonts, Ufex.API.Tree.TreeViewIcon.Text);
		AddObjectGroup("Images", images, Ufex.API.Tree.TreeViewIcon.Image);
		AddObjectGroup("Streams", streams, Ufex.API.Tree.TreeViewIcon.Binary);
		AddObjectGroup("Other Objects", other, Ufex.API.Tree.TreeViewIcon.Object);

		// XRef table node
		TreeNodes.Add(new XRefTableNode(reader.XRefEntries, reader.XRefOffset, reader.UsesXRefStream));

		// Trailer node
		TreeNodes.Add(new TrailerNode(reader.Trailer));
	}

	private void AddObjectGroup(string groupName, List<IndirectObject> objects, Ufex.API.Tree.TreeViewIcon icon)
	{
		if (objects.Count == 0) return;

		var groupNode = new Ufex.API.Tree.TreeNode($"{groupName} ({objects.Count})", icon, icon);
		foreach (var obj in objects)
		{
			groupNode.Nodes.Add(PdfObjectNode.FromObject(obj));
		}
		TreeNodes.Add(groupNode);
	}

	private static uint GetObjectColor(IndirectObject obj)
	{
		return obj.TypeName switch
		{
			"Catalog" => 0xFF66AADD,
			"Pages" => 0xFF88BB66,
			"Page" => 0xFF88CC88,
			"Font" => 0xFFDDAA44,
			"FontDescriptor" => 0xFFCC9933,
			"XObject" => 0xFFDD6688,
			"XRef" => 0xFFCC8844,
			"ObjStm" => 0xFFAA88CC,
			"Annot" => 0xFF99AADD,
			"Metadata" => 0xFF88CCCC,
			_ => obj.Value is PdfStream ? 0xFFBB88BB : 0xFFAAAAAA
		};
	}

	private static string FormatMetaKey(string key) => key switch
	{
		"CreationDate" => "Creation Date",
		"ModDate" => "Modification Date",
		_ => key
	};
}