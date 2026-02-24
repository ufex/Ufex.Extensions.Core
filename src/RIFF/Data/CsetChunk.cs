using Ufex.API;

namespace Ufex.Extensions.Core.RIFF.Data;

/// <summary>
/// CSET - Character Set chunk
/// </summary>
internal class CsetChunk : Chunk
{
	/// <summary>
	/// Specifies the code page used for file elements. If the CSET 
	/// chunk is not present, or if this field has value zero, assume
	/// standard ISO 8859/1 code page
	/// </summary>
	public UInt16 CodePage { get; init; }

	/// <summary>
	/// Specifies the country code used for file elements.
	/// </summary>
	public UInt16 CountryCode { get; init; }

	/// <summary>
	/// Specify the language and dialect used for file elements.
	/// </summary>
	public UInt16 Language { get; init; }

	/// <summary>
	/// Specifies the dialect used for file elements. 
	/// The interpretation of this field depends on the value of the Language field. 
	/// For example, if the Language field specifies English, the Dialect field could 
	/// specify US English, UK English, etc.
	/// </summary>
	public UInt16 Dialect { get; init; }

	public CsetChunk(FileReader fr) : base(fr)
	{
		CodePage = fr.ReadUInt16();
		CountryCode = fr.ReadUInt16();
		Language = fr.ReadUInt16();
		Dialect = fr.ReadUInt16();
	}
}