using Ufex.API;

namespace Ufex.FileTypes.BMP.Data;

/// <summary>
/// CIEXYZ - CIE XYZ color value
/// </summary>
internal struct CIEXYZ
{
	public int CieXyzX;
	public int CieXyzY;
	public int CieXyzZ;

	public CIEXYZ(FileReader fr)
	{
		CieXyzX = fr.ReadInt32();
		CieXyzY = fr.ReadInt32();
		CieXyzZ = fr.ReadInt32();
	}

	public CIEXYZ(BinaryReader br)
	{
		CieXyzX = br.ReadInt32();
		CieXyzY = br.ReadInt32();
		CieXyzZ = br.ReadInt32();
	}

	public override readonly string ToString()
	{
		return $"({CieXyzX}, {CieXyzY}, {CieXyzZ})";
	}
}
