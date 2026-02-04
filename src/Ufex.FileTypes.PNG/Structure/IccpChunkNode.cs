using Ufex.API;
using Ufex.API.Tree;
using Ufex.FileTypes.PNG.Data;

namespace Ufex.FileTypes.PNG.Structure;

/// <summary>
/// iCCP - Embedded ICC profile
/// </summary>
class IccpChunkNode : ChunkNode
{
	public IccpChunkNode(IccpChunk chunk)
		: base(chunk, "iCCP", "Embedded ICC profile", TreeViewIcon.Section)
	{
	}

	public override object[][] GetRows()
	{
		var d = (IccpChunk)Chunk;
		return [
			["Profile Name", d.ProfileName],
			["Compression Method", d.CompressionMethod, d.CompressionMethod == 0 ? "Deflate" : "Unknown"],
			["Compressed Size", d.CompressedProfile.Length, d.CompressedProfile.Length.ToString() + " bytes"],
			["Profile Size", d.Profile != null ? d.Profile.Length : 0, d.Profile != null ? (d.Profile.Length.ToString() + " bytes") : ""],
		];
	}
}
