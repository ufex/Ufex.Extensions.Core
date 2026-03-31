using Ufex.API;

namespace Ufex.Extensions.Core.PNG.Data;

/// <summary>
/// eXIf - Exchangeable image file format metadata.
/// Data contains a TIFF header followed by IFD structures.
/// </summary>
internal class ExifChunk : Chunk
{
	public byte[] ExifData { get; init; }

	public ExifChunk(FileReader fr) : base(fr)
	{
		ExifData = fr.ReadBytes((int)Length);
	}
}
