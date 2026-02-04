using System;
using System.Collections.Generic;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Visual;
using Ufex.FileTypes.BMP.Data;
using Ufex.FileTypes.BMP.Structure;

namespace Ufex.FileTypes.BMP;

/// <summary>
/// Ufex FileType module for Windows Icon (ICO) and Cursor (CUR) files
/// </summary>
public class WindowsIconFileType : FileType
{
	public WindowsIconFileType()
	{
		Description = "Windows Icon (ICO)";
		Log.SetLogName("ICO.log");

		ShowTechnical = true;
		ShowGraphic = true;
		ShowFileCheck = true;
	}

	public override bool ProcessFile()
	{
		// Parse the file
		var reader = new IcoStreamReader(FileInStream, Log, ValidationReport);
		bool success = reader.Read();

		// Build UI components
		BuildQuickInfo(reader);
		BuildVisuals(reader);
		BuildStructure(reader);

		return success;
	}

	private void BuildQuickInfo(IcoStreamReader reader)
	{
		if (reader.Header == null) return;

		string fileType = reader.Header.Type == Signatures.ICO_TYPE ? "Icon" : "Cursor";
		QuickInfoTable.AddRow("Type", fileType);
		QuickInfoTable.AddRow("Number of Images", reader.Header.Count.ToString());

		// Show dimensions of each image
		for (int i = 0; i < reader.Entries.Count; i++)
		{
			var entry = reader.Entries[i];
			string dimensions = $"{entry.ActualWidth}x{entry.ActualHeight}, {entry.BitCount}-bit";
			QuickInfoTable.AddRow($"Image {i + 1}", dimensions);
		}
	}

	private void BuildVisuals(IcoStreamReader reader)
	{
		var spans = new List<FileSpan>();

		// Icon directory header span
		if (reader.Header != null)
		{
			spans.Add(new FileSpan
			{
				StartPosition = reader.Header.Offset,
				EndPosition = reader.Header.Offset + 6,
				Name = "Icon Directory"
			});
		}

		// Directory entries span
		if (reader.Entries.Count > 0)
		{
			long entriesStart = reader.Entries[0].Offset;
			long entriesEnd = reader.Entries[^1].Offset + 16;  // Each entry is 16 bytes
			spans.Add(new FileSpan
			{
				StartPosition = entriesStart,
				EndPosition = entriesEnd,
				Name = "Directory Entries"
			});
		}

		// Image data spans
		for (int i = 0; i < reader.Entries.Count; i++)
		{
			var entry = reader.Entries[i];
			spans.Add(new FileSpan
			{
				StartPosition = entry.ImageOffset,
				EndPosition = entry.ImageOffset + entry.BytesInRes,
				Name = $"Image {i + 1} ({entry.ActualWidth}x{entry.ActualHeight})"
			});
		}

		var fileMap = new FileMap(spans.ToArray(), (ulong)FileInStream.Length);
		VisualsList.Add(fileMap);
	}

	private void BuildStructure(IcoStreamReader reader)
	{
		// Icon Directory header node
		if (reader.Header != null)
		{
			TreeNodes.Add(new IconDirNode(reader.Header));
		}

		// Directory Entries folder
		if (reader.Entries.Count > 0)
		{
			var entriesFolder = new TreeNode("Directory Entries", TreeViewIcon.FolderClosed, TreeViewIcon.FolderOpen);
			for (int i = 0; i < reader.Entries.Count; i++)
			{
				entriesFolder.Nodes.Add(new IconDirEntryNode(reader.Entries[i], i));
			}
			TreeNodes.Add(entriesFolder);
		}

		// Icon Images folder
		if (reader.Images.Count > 0)
		{
			var imagesFolder = new TreeNode("Images", TreeViewIcon.FolderClosed, TreeViewIcon.FolderOpen);
			foreach (var image in reader.Images)
			{
				imagesFolder.Nodes.Add(new IconImageNode(image));
			}
			TreeNodes.Add(imagesFolder);
		}
	}
}
