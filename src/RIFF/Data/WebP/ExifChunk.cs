using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data.WebP;

/// <summary>
/// EXIF - Exif metadata chunk. Contains image metadata in Exif format.
/// </summary>
internal class ExifChunk : Chunk
{
	/// <summary>
	/// The offset in the file where the Exif metadata begins
	/// (after the chunk header).
	/// </summary>
	public long DataOffset { get; init; }

	/// <summary>
	/// The size of the Exif metadata in bytes.
	/// </summary>
	public UInt32 DataSize => Size;

	public ExifChunk(FileReader fr) : base(fr)
	{
		DataOffset = fr.BaseStream.Position;
	}
}
