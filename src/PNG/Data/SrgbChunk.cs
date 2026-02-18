using Ufex.API;

namespace Ufex.Extensions.Core.PNG.Data;

/// <summary>
/// sRGB - Standard RGB colour space
/// </summary>
internal class SrgbChunk : Chunk
{
	public byte RenderingIntent { get; init; }

	public SrgbChunk(FileReader fr) : base(fr)
	{
		RenderingIntent = fr.ReadByte();
	}
}
