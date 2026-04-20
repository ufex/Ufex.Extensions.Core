using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.Heif;

/// <summary>
/// mdcv — Mastering Display Colour Volume property. HDR metadata describing
/// the colour gamut and luminance range of the mastering display.
/// </summary>
internal class MdcvBox : Box
{
	/// <summary>
	/// Display primaries x chromaticity × 50000, in order [R, G, B].
	/// </summary>
	public UInt16[] DisplayPrimariesX { get; init; }

	/// <summary>
	/// Display primaries y chromaticity × 50000, in order [R, G, B].
	/// </summary>
	public UInt16[] DisplayPrimariesY { get; init; }

	/// <summary>
	/// White point x chromaticity × 50000.
	/// </summary>
	public UInt16 WhitePointX { get; init; }

	/// <summary>
	/// White point y chromaticity × 50000.
	/// </summary>
	public UInt16 WhitePointY { get; init; }

	/// <summary>
	/// Maximum display mastering luminance in 0.0001 nit units.
	/// </summary>
	public UInt32 MaxDisplayMasteringLuminance { get; init; }

	/// <summary>
	/// Minimum display mastering luminance in 0.0001 nit units.
	/// </summary>
	public UInt32 MinDisplayMasteringLuminance { get; init; }

	public MdcvBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, false)
	{
		DisplayPrimariesX = new UInt16[3];
		DisplayPrimariesY = new UInt16[3];

		for (Int32 i = 0; i < 3; i++)
		{
			DisplayPrimariesX[i] = fr.ReadUInt16();
			DisplayPrimariesY[i] = fr.ReadUInt16();
		}

		WhitePointX = fr.ReadUInt16();
		WhitePointY = fr.ReadUInt16();
		MaxDisplayMasteringLuminance = fr.ReadUInt32();
		MinDisplayMasteringLuminance = fr.ReadUInt32();
	}
}
