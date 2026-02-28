using System;
using Ufex.API;
using Ufex.API.Tree;
using Ufex.API.Tables;
using Ufex.Extensions.Core.PNG.Data;

namespace Ufex.Extensions.Core.PNG.Structure;

/// <summary>
/// pHYs - Physical pixel dimensions
/// </summary>
class PhysChunkNode : ChunkNode
{
	public PhysChunkNode(PhysChunk chunk) 
		: base(chunk, "pHYs", "Physical pixel dimensions", TreeViewIcon.Properties)
	{
	}

	readonly static Dictionary<byte, string> UNITS = new Dictionary<byte, string>()
	{
		{ 0x00, "Unknown" },
		{ 0x01, "Metre" }
	};

	public override object[][] GetRows()
	{
		var d = (PhysChunk)Chunk;
		object[][] rows = [
			["Pixels per unit X", d.PixelsPerUnitX],
			["Pixels per unit Y", d.PixelsPerUnitY],
			["Unit", d.Unit, UNITS.ContainsKey(d.Unit) ? UNITS[d.Unit] : ""],
		];
		return rows;
	}
}
