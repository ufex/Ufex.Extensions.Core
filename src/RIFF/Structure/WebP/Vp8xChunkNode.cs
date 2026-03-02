using Ufex.API.Tree;
using Ufex.Extensions.Core.RIFF.Data.WebP;

namespace Ufex.Extensions.Core.RIFF.Structure.WebP;

/// <summary>
/// Tree node for the VP8X extended file header chunk.
/// </summary>
class Vp8xChunkNode : ChunkNode
{
	public Vp8xChunkNode(Vp8xChunk chunk)
		: base(chunk, chunk.ChunkIDString, "Extended header", TreeViewIcon.Header)
	{
	}

	public override object[][] GetRows()
	{
		var d = (Vp8xChunk)_chunk;

		string features = string.Join(", ", GetFeatureList(d));
		if (features.Length == 0) features = "None";

		return [
			["Flags", d.Flags, features],
			["Reserved", d.Reserved],
			["CanvasWidthMinusOne", d.CanvasWidthMinusOne, $"{d.CanvasWidth} px"],
			["CanvasHeightMinusOne", d.CanvasHeightMinusOne, $"{d.CanvasHeight} px"]
		];
	}

	private static List<string> GetFeatureList(Vp8xChunk chunk)
	{
		var features = new List<string>();
		if (chunk.HasIccProfile) features.Add("ICC Profile");
		if (chunk.HasAlpha) features.Add("Alpha");
		if (chunk.HasExif) features.Add("Exif");
		if (chunk.HasXmp) features.Add("XMP");
		if (chunk.HasAnimation) features.Add("Animation");
		return features;
	}
}
