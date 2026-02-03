using System;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.FileTypes.PNG.Data;

namespace Ufex.FileTypes.PNG.Structure;

/// <summary>
/// tIME - Image last-modification time
/// </summary>
class TimeChunkNode : ChunkNode
{
	public TimeChunkNode(TimeChunk chunk) 
		: base(chunk, "tIME", "Image last-modification time", TreeViewIcon.Section)
	{
	}

	public override object[][] GetRows()
	{
		var d = (TimeChunk)Chunk;
		object[][] rows = [
			["Year", d.Year],
			["Month", d.Month],
			["Day", d.Day],
			["Hour", d.Hour],
			["Minute", d.Minute],
			["Second", d.Second],
		];
		return rows;
	}
}
