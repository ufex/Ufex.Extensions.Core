using Ufex.API;
using Ufex.API.Tree;
using Ufex.FileTypes.PNG.Data;

namespace Ufex.FileTypes.PNG.Structure;

/// <summary>
/// IHDR - Image header
/// </summary>
class IhdrChunkNode : ChunkNode
{
	const string Unknown = "Unknown";
	readonly Dictionary<byte, string> _interlaceMethods = new()
	{
		{ 0x00, "No interlace" },
		{ 0x01, "Adam7 interlace" }
	};

	public IhdrChunkNode(IhdrChunk chunk)
		: base(chunk, "IHDR", "Image header", TreeViewIcon.Section)
	{
	}

	public override object[][] GetRows()
	{
		var d = (IhdrChunk)Chunk;
		return [
			["Width", d.Width, d.Width.ToString() + " pixels"],
			["Height", d.Height, d.Height.ToString() + " pixels"],
			["Bit Depth", d.BitDepth],
			["Color Type", d.ColorType],
			["Compression Method", d.CompressionMethod, d.CompressionMethod == 0 ? "Deflate" : Unknown],
			["Filter Method", d.FilterMethod, d.FilterMethod == 0 ? "Adaptive" : Unknown],
			["Interlace Method", d.InterlaceMethod, _interlaceMethods.ContainsKey(d.InterlaceMethod) ? _interlaceMethods[d.InterlaceMethod] : Unknown],
		];
	}
}
