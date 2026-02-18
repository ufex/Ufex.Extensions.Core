using Ufex.API;

namespace Ufex.Extensions.Core.BMP.Data;

/// <summary>
/// BITMAPV4HEADER - Bitmap Info Header v4 (108 bytes)
/// </summary>
internal class BitmapV4Header : BitmapInfoHeader
{
	/// <summary>
	/// Red channel bit mask
	/// </summary>
	public uint RedMask { get; init; }

	/// <summary>
	/// Green channel bit mask
	/// </summary>
	public uint GreenMask { get; init; }

	/// <summary>
	/// Blue channel bit mask
	/// </summary>
	public uint BlueMask { get; init; }

	/// <summary>
	/// Alpha channel bit mask
	/// </summary>
	public uint AlphaMask { get; init; }

	/// <summary>
	/// Color space type
	/// </summary>
	public uint CSType { get; init; }

	/// <summary>
	/// CIE XYZ color space endpoints
	/// </summary>
	public CIEXYZTRIPLE Endpoints { get; init; }

	/// <summary>
	/// Gamma red channel value
	/// </summary>
	public uint GammaRed { get; init; }

	/// <summary>
	/// Gamma green channel value
	/// </summary>
	public uint GammaGreen { get; init; }

	/// <summary>
	/// Gamma blue channel value
	/// </summary>
	public uint GammaBlue { get; init; }

	public BitmapV4Header(FileReader fr) : base(fr)
	{
		RedMask = fr.ReadUInt32();
		GreenMask = fr.ReadUInt32();
		BlueMask = fr.ReadUInt32();
		AlphaMask = fr.ReadUInt32();
		CSType = fr.ReadUInt32();
		Endpoints = new CIEXYZTRIPLE(fr);
		GammaRed = fr.ReadUInt32();
		GammaGreen = fr.ReadUInt32();
		GammaBlue = fr.ReadUInt32();
	}
}
