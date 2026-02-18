using Ufex.API;

namespace Ufex.Extensions.Core.BMP.Data;

/// <summary>
/// BITMAPFILEHEADER - 14 bytes
/// The file header at the start of every BMP file
/// </summary>
internal class BitmapFileHeader
{
	/// <summary>
	/// File offset where this header starts
	/// </summary>
	public long Offset { get; init; }

	/// <summary>
	/// File type signature, must be 0x4D42 ('BM')
	/// </summary>
	public ushort Type { get; init; }

	/// <summary>
	/// Size of the BMP file in bytes
	/// </summary>
	public uint Size { get; init; }

	/// <summary>
	/// Reserved, must be zero
	/// </summary>
	public ushort Reserved1 { get; init; }

	/// <summary>
	/// Reserved, must be zero
	/// </summary>
	public ushort Reserved2 { get; init; }

	/// <summary>
	/// Offset from beginning of file to the beginning of the bitmap data
	/// </summary>
	public uint OffBits { get; init; }

	public BitmapFileHeader(FileReader fr)
	{
		Offset = fr.BaseStream.Position;
		Type = fr.ReadUInt16();
		Size = fr.ReadUInt32();
		Reserved1 = fr.ReadUInt16();
		Reserved2 = fr.ReadUInt16();
		OffBits = fr.ReadUInt32();
	}
}
