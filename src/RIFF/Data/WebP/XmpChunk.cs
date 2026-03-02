using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data.WebP;

/// <summary>
/// XMP  - XMP metadata chunk. Contains image metadata in XMP format.
/// Note that the fourth character in the FourCC is an ASCII space (0x20).
/// </summary>
internal class XmpChunk : Chunk
{
	/// <summary>
	/// The offset in the file where the XMP metadata begins
	/// (after the chunk header).
	/// </summary>
	public long DataOffset { get; init; }

	/// <summary>
	/// The size of the XMP metadata in bytes.
	/// </summary>
	public UInt32 DataSize => Size;

	public XmpChunk(FileReader fr) : base(fr)
	{
		DataOffset = fr.BaseStream.Position;
	}
}
