using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using Ufex.API;
using Ufex.API.Tables;
using Ufex.API.Tree;
using Ufex.API.Visual;

using Ufex.Extensions.Core.RIFF.Data;
using Ufex.Extensions.Core.RIFF.Structure;

namespace Ufex.Extensions.Core.RIFF;

/// <summary>
/// Summary description for RIFF.
/// </summary>
public class RiffFileType : FileType
{
	protected FileMap? Map { get; set; }

	public RiffFileType()
	{
		ShowGraphic = true;
		ShowTechnical = true;
		ShowFileCheck = true;
	}

	public override bool ProcessFile()
	{

		RiffStreamReader riffReader = new RiffStreamReader(FileInStream, Log, ValidationReport);
		bool result = riffReader.Read();

		BuildQuickInfo(riffReader);
		BuildVisuals(riffReader);
		BuildStructure(riffReader);

		return result;
	}

	public void BuildQuickInfo(RiffStreamReader riffReader)
	{
		var quickInfo = QuickInfoTable;

	}

	protected void BuildVisuals(RiffStreamReader riffReader)
	{
		var spans = new List<FileSpan>();
		var chunks = new List<Chunk>();
		if(riffReader.Chunks.Count == 1)
		{
			var firstChunk = riffReader.Chunks[0];
			if(firstChunk is RiffChunk riffChunk)
			{
				chunks = riffChunk.Chunks;
			}
			else if(firstChunk is ListChunk listChunk)
			{
				chunks = listChunk.SubChunks;
			}
			else
			{
				return;
			}
		}
		else 
		{
			chunks = riffReader.Chunks;
		}

		foreach(Chunk chunk in chunks)
		{
			ChunkNode node = ChunkNode.FromChunk(chunk);
			spans.Add(new FileSpan
			{
				StartPosition = chunk.Offset,
				EndPosition = chunk.Offset + chunk.Size + 8,
				Name = node.Description
			});
		}
		
		// Create the FileMap from collected spans
		Map = new FileMap(spans.ToArray(), (ulong)FileInStream.Length);
		VisualsList.Add(Map);
	}

	protected void BuildStructure(RiffStreamReader riffReader) 
	{
		foreach(Chunk chunk in riffReader.Chunks)
		{
			Log.Info($"Processing chunk at position {chunk.Offset}, type {chunk.GetType().Name}");
			ChunkNode node = ChunkNode.FromChunk(chunk);
			TreeNodes.Add(node);
		}
	}
}