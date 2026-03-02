using System;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.API.Format;
using Ufex.Extensions.Core.RIFF.Data;

namespace Ufex.Extensions.Core.RIFF.Structure;

internal class ChunkNode : TreeNode
{

	private static readonly Dictionary<Type, Type> CHUNK_NODE_TYPES = new()
	{
		{ typeof(ZStrChunk), typeof(ZStrChunkNode) },
		{ typeof(ListChunk), typeof(ListChunkNode) },
		{ typeof(InfoListChunk), typeof(ListChunkNode) },
		{ typeof(RIFF.Data.Wave.AdtlListChunk), typeof(ListChunkNode) },
		{ typeof(RIFF.Data.Wave.FmtChunk), typeof(Structure.Wave.FmtChunkNode) },
		{ typeof(RIFF.Data.Wave.FactChunk), typeof(Structure.Wave.FactChunkNode) },
		{ typeof(RIFF.Data.Wave.DataChunk), typeof(Structure.Wave.DataChunkNode) },
		{ typeof(RIFF.Data.Wave.CueChunk), typeof(Structure.Wave.CueChunkNode) },
		{ typeof(RIFF.Data.Wave.PlstChunk), typeof(Structure.Wave.PlstChunkNode) },
		{ typeof(RIFF.Data.Wave.SmplChunk), typeof(Structure.Wave.SmplChunkNode) },
		{ typeof(RIFF.Data.Wave.InstChunk), typeof(Structure.Wave.InstChunkNode) },
		{ typeof(RIFF.Data.Wave.LablChunk), typeof(Structure.Wave.LablChunkNode) },
		{ typeof(RIFF.Data.Wave.NoteChunk), typeof(Structure.Wave.NoteChunkNode) },
		{ typeof(RIFF.Data.Wave.LtxtChunk), typeof(Structure.Wave.LtxtChunkNode) },
		{ typeof(RIFF.Data.WebP.Vp8xChunk), typeof(Structure.WebP.Vp8xChunkNode) },
		{ typeof(RIFF.Data.WebP.Vp8Chunk), typeof(Structure.WebP.Vp8ChunkNode) },
		{ typeof(RIFF.Data.WebP.Vp8lChunk), typeof(Structure.WebP.Vp8lChunkNode) },
		{ typeof(RIFF.Data.WebP.AnimChunk), typeof(Structure.WebP.AnimChunkNode) },
		{ typeof(RIFF.Data.WebP.AnmfChunk), typeof(Structure.WebP.AnmfChunkNode) },
		{ typeof(RIFF.Data.WebP.AlphChunk), typeof(Structure.WebP.AlphChunkNode) },
		{ typeof(RIFF.Data.WebP.IccpChunk), typeof(Structure.WebP.IccpChunkNode) },
		{ typeof(RIFF.Data.WebP.ExifChunk), typeof(Structure.WebP.ExifChunkNode) },
		{ typeof(RIFF.Data.WebP.XmpChunk), typeof(Structure.WebP.XmpChunkNode) }
	};

	protected Chunk _chunk;

	public string Description { get; protected set; }

	public override Ufex.API.Visual.Visual[] Visuals
	{
		get { return [ new DataGridVisual(TableData(), "Data") ]; }
	}

	public ChunkNode(Chunk chunk, string chunkType, string chunkDescription, TreeViewIcon icon) 
		: base(chunkType, icon, icon)
	{
		_chunk = chunk;
		Description = chunkType + " - " + chunkDescription;
		if(chunk is RiffChunk riffChunk)
		{
			foreach(Chunk subChunk in riffChunk.Chunks)
			{
				Nodes.Add(FromChunk(subChunk));
			}
		}
		else if(chunk is ListChunk listChunk)
		{
			foreach(Chunk subChunk in listChunk.SubChunks)
			{
				Nodes.Add(FromChunk(subChunk));
			}
		}
	}

	public static ChunkNode FromChunk(Chunk chunk)
	{
		if (!CHUNK_NODE_TYPES.TryGetValue(chunk.GetType(), out var chunkClass))
		{
			var fallbackType = string.IsNullOrWhiteSpace(chunk.ChunkIDString) ? chunk.GetType().Name : chunk.ChunkIDString;
			return new ChunkNode(chunk, fallbackType, "Unknown", TreeViewIcon.NullIcon);			
		}
		foreach (var ctor in chunkClass.GetConstructors())
		{
			var parameters = ctor.GetParameters();
			if (parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(chunk.GetType()))
			{
				return (ChunkNode)ctor.Invoke(new object[] { chunk });
			}
		}
		throw new InvalidOperationException("No suitable constructor found for chunk node.");
	}

	public virtual DynamicTableData TableData()
	{ 
		DynamicTableData td = new DynamicTableData(4, "RIFF.PropertyValueDescriptionOffset");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");
		td.SetColumn(3, "Offset");

		td.AddRow("ID", _chunk.ChunkID, _chunk.ChunkIDString, (UInt32)(_chunk.Offset));
		td.AddRow("Size", _chunk.Size, ByteCountFormatter.Format(_chunk.Size), (UInt32)(_chunk.Offset) + 4);

		var rows = GetRows();
		long offset = _chunk.Offset + 8; // Skip length and type fields
		for(int i = 0; i < rows.Length; i++)
		{
			td.AddRow(rows[i][0], rows[i][1], rows[i].Length > 2 ? rows[i][2] : "", new FileOffset((UInt32)offset));
			offset += ByteUtil.GetObjectSize(rows[i][1]);
		}
		return td;
	}

	public virtual object[][] GetRows()
	{
		return [];
	}
}
