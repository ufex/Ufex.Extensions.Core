using Ufex.API;

namespace Ufex.Extensions.Core.PNG.Data;

/// <summary>
/// pHYs - Physical pixel dimensions
/// </summary>
internal class PhysChunk : Chunk
{
	public static readonly Dictionary<byte, string> Units = new Dictionary<byte, string>()
	{
		{ 0x00, "Unknown" },
		{ 0x01, "Metre" }
	};

	public uint PixelsPerUnitX { get; init; }
	public uint PixelsPerUnitY { get; init; }
	public byte Unit { get; init; }

	public PhysChunk(FileReader fr) : base(fr)
	{
		PixelsPerUnitX = fr.ReadUInt32();
		PixelsPerUnitY = fr.ReadUInt32();
		Unit = fr.ReadByte();
	}
}
