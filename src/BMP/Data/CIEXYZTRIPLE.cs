using Ufex.API;

namespace Ufex.Extensions.Core.BMP.Data;

/// <summary>
/// CIEXYZTRIPLE - Three CIE XYZ values for red, green, and blue endpoints
/// </summary>
internal struct CIEXYZTRIPLE
{
	public CIEXYZ CieXyzRed;
	public CIEXYZ CieXyzGreen;
	public CIEXYZ CieXyzBlue;

	public CIEXYZTRIPLE(FileReader fr)
	{
		CieXyzRed = new CIEXYZ(fr);
		CieXyzGreen = new CIEXYZ(fr);
		CieXyzBlue = new CIEXYZ(fr);
	}

	public CIEXYZTRIPLE(BinaryReader br)
	{
		CieXyzRed = new CIEXYZ(br);
		CieXyzGreen = new CIEXYZ(br);
		CieXyzBlue = new CIEXYZ(br);
	}
}
