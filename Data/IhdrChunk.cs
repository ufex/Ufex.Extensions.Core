using System.IO;
using Ufex.API;

namespace Ufex.FileTypes.PNG.Data;

/// <summary>
/// IHDR - Image header
/// </summary>
internal class IhdrChunk : Chunk
{
	public UInt32 Width { get; init; }
	public UInt32 Height { get; init; }
	public Byte BitDepth { get; init; }
	public Byte ColorType { get; init; }
	public Byte CompressionMethod { get; init; }
	public Byte FilterMethod { get; init; }
	public Byte InterlaceMethod { get; init; }

	public IhdrChunk(FileReader fr) : base(fr)
	{
		Width = fr.ReadUInt32();
		Height = fr.ReadUInt32();
		BitDepth = fr.ReadByte();
		ColorType = fr.ReadByte();
		CompressionMethod = fr.ReadByte();
		FilterMethod = fr.ReadByte();
		InterlaceMethod = fr.ReadByte();
	}
}