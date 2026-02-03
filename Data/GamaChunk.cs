using Ufex.API;

namespace Ufex.FileTypes.PNG.Data;

/// <summary>
/// gAMA - Image gamma
/// </summary>
internal class GamaChunk : Chunk
{
	public uint Gamma { get; init; }

	public GamaChunk(FileReader fr) : base(fr)
	{
		Gamma = fr.ReadUInt32();
	}
}
