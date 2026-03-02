using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data.WebP;

/// <summary>
/// ICCP - ICC color profile chunk. Contains an ICC color profile.
/// This chunk must appear before the image data.
/// If not present, sRGB is assumed.
/// </summary>
internal class IccpChunk : Chunk
{
	/// <summary>
	/// The offset in the file where the ICC profile data begins
	/// (after the chunk header).
	/// </summary>
	public long DataOffset { get; init; }

	/// <summary>
	/// The size of the ICC profile data in bytes.
	/// </summary>
	public UInt32 DataSize => Size;

	public IccpChunk(FileReader fr) : base(fr)
	{
		DataOffset = fr.BaseStream.Position;
	}
}
