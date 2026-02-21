using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data;

internal class CsetChunk : Chunk
{
	public UInt16 CodePage { get; init; }
	public UInt16 CountryCode { get; init; }
	public UInt16 Language { get; init; }
	public UInt16 Dialect { get; init; }

	public CsetChunk(FileReader fr) : base(fr)
	{
		CodePage = fr.ReadUInt16();
		CountryCode = fr.ReadUInt16();
		Language = fr.ReadUInt16();
		Dialect = fr.ReadUInt16();
	}
}