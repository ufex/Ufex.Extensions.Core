using Ufex.API;

namespace Ufex.Extensions.Core.BMP.Data;

/// <summary>
/// RGBQUAD - Color table entry (4 bytes)
/// </summary>
internal struct RGBQUAD
{
	public byte Blue;
	public byte Green;
	public byte Red;
	public byte Reserved;

	public RGBQUAD(FileReader fr)
	{
		Blue = fr.ReadByte();
		Green = fr.ReadByte();
		Red = fr.ReadByte();
		Reserved = fr.ReadByte();
	}

	public RGBQUAD(BinaryReader br)
	{
		Blue = br.ReadByte();
		Green = br.ReadByte();
		Red = br.ReadByte();
		Reserved = br.ReadByte();
	}

	public override readonly string ToString()
	{
		return $"#{Red:X2}{Green:X2}{Blue:X2}";
	}
}
