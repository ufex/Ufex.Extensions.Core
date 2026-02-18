using System;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.API.Visual;
using Ufex.API.Format;
using Ufex.Extensions.Core.PNG.Data;

namespace Ufex.Extensions.Core.PNG.Structure;

internal class ChunkNode : TreeNode
{
	public Chunk Chunk;

	public string Description { get; protected set; }

	public override Ufex.API.Visual.Visual[] Visuals
	{
		get { return [ new DataGridVisual(TableData(), "Data") ]; }
	}

	public ChunkNode(Chunk chunk, string chunkType, string chunkDescription, TreeViewIcon icon) 
		: base(chunkType, icon, icon)
	{
		Chunk = chunk;
		Description = chunkType + " - " + chunkDescription;
	}

	public static ChunkNode FromChunk(Chunk chunk)
	{
		var chunkType = chunk.ChunkTypeString;
		var nodeTypeName = GetNodeTypeName(chunkType);

		var assembly = typeof(ChunkNode).Assembly;
		var nodeType = assembly.GetType($"Ufex.Extensions.Core.PNG.Structure.{nodeTypeName}");
		if (nodeType != null && nodeType.IsSubclassOf(typeof(ChunkNode)))
		{
			foreach (var ctor in nodeType.GetConstructors())
			{
				var parameters = ctor.GetParameters();
				if (parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(chunk.GetType()))
				{
					return (ChunkNode)ctor.Invoke(new object[] { chunk });
				}
			}
		}

		var fallbackType = string.IsNullOrWhiteSpace(chunkType) ? chunk.GetType().Name : chunkType;
		return new ChunkNode(chunk, fallbackType, "Unknown", TreeViewIcon.NullIcon);
	}

	private static string GetNodeTypeName(string chunkType)
	{
		if (string.IsNullOrWhiteSpace(chunkType))
			return "ChunkNode";

		// Semantic exceptions / established naming.
		return chunkType switch
		{
			"tEXt" => nameof(TextChunkNode),
			"pHYs" => nameof(PhysChunkNode),
			"tIME" => nameof(TimeChunkNode),
			_ => ToPascalish(chunkType) + "ChunkNode",
		};
	}

	private static string ToPascalish(string chunkType)
	{
		// Desired convention: first letter upper, rest lower (e.g. "tRNS" => "Trns").
		var lower = chunkType.ToLowerInvariant();
		if (lower.Length == 1)
			return char.ToUpperInvariant(lower[0]).ToString();
		return char.ToUpperInvariant(lower[0]) + lower.Substring(1);
	}

	public virtual DynamicTableData TableData()
	{ 
		DynamicTableData td = new DynamicTableData(4, "Zip.PropertyValueDescription");
		td.SetColumn(0, "Property");
		td.SetColumn(1, "Value");
		td.SetColumn(2, "Description");
		td.SetColumn(3, "Offset");
		
		td.AddRow("Length", Chunk.Length, ByteCountFormatter.Format(Chunk.Length), (UInt32)(Chunk.Offset));
		td.AddRow("Chunk Type", Chunk.ChunkType, Chunk.ChunkTypeString, (UInt32)(Chunk.Offset) + 4);

		var rows = GetRows();
		long offset = Chunk.Offset + 8; // Skip length and type fields
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
