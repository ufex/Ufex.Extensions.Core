using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Visual;
using Ufex.Extensions.Core.ZIP.Data;
using Ufex.Extensions.Core.ZIP.Structure;

namespace Ufex.Extensions.Core.ZIP;

/// <summary>
/// FileType implementation for ZIP files.
/// </summary>
public class ZipFileType : FileType
{
	protected List<Section> Parts { get; set; }

	protected FileMap? Map { get; set; }

	public ZipFileType()
	{
		ShowTechnical = true;
		ShowGraphic = true;
		ShowFileCheck = true;
		Parts = new List<Section>();
	}

	public override bool ProcessFile()
	{	
		ZipStreamReader zipReader = new ZipStreamReader(FileInStream, Log, ValidationReport);
		bool result = zipReader.Read();

		BuildQuickInfo(zipReader);
		BuildVisuals(zipReader);
		BuildStructure(zipReader);
		return result;
	}

	protected void BuildQuickInfo(ZipStreamReader zipReader)
	{
		var parts = zipReader.Parts;
		var quickInfo = QuickInfoTable;
		quickInfo.AddRow("Number of Files", parts.FindAll(p => p is CompressedFile).Count.ToString());
		quickInfo.AddRow("Compression Methods", string.Join(", ", parts.OfType<CompressedFile>().Select(f => Constants.CompressionMethodDescription(f.Header.CompressionMethod)).Distinct()));
		foreach(var part in parts)
		{
			if(part is EndOfCentralDirectoryRecord eocdRecord)
			{
				quickInfo.AddRow("ZIP Comment", eocdRecord.ZIPFileCommentText);
			}
		}
	}

	protected void BuildVisuals(ZipStreamReader zipReader)
	{
		var spans = new List<FileSpan>();
		foreach(Section segment in zipReader.Parts)
		{
			SectionNode node = SectionNode.FromSection(segment);
			spans.Add(new FileSpan
			{
				StartPosition = segment.StartPosition,
				EndPosition = segment.EndPosition,
				Name = node.Description
			});
		}
		
		// Create the FileMap from collected spans
		Map = new FileMap(spans.ToArray(), (ulong)FileInStream.Length);
		VisualsList.Add(Map);
	}

	protected void BuildStructure(ZipStreamReader zipReader) 
	{
		TreeNode tnFiles = new TreeNode("Files", TreeViewIcon.FolderClosed, TreeViewIcon.FolderOpen);
		TreeNode tnOther = new TreeNode("Other Data", TreeViewIcon.FolderClosed, TreeViewIcon.FolderOpen);
	
		foreach(Section segment in zipReader.Parts)
		{
			Log.LogInformation($"Processing segment at position {segment.StartPosition}, type {segment.GetType().Name}");
			SectionNode node = SectionNode.FromSection(segment);
			switch(segment)
			{
				case CompressedFile compFile:
					tnFiles.Nodes.Add(node);
					break;
				default:
					tnOther.Nodes.Add(node);
					break;
			}
		}
		
		TreeNodes.Add(tnFiles);
		TreeNodes.Add(tnOther);
	}

}
