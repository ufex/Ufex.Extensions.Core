using System;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.FileTypes.PNG.Data;

namespace Ufex.FileTypes.PNG.Structure;

/// <summary>
/// hIST - Image histogram
/// </summary>
class HistChunkNode : ChunkNode
{
	public HistChunkNode(HistChunk chunk) 
		: base(chunk, "hIST", "Image histogram", TreeViewIcon.Section)
	{
	}

	// TODO
}