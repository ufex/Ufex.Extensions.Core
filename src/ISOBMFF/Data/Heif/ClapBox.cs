using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.Heif;

/// <summary>
/// clap — Clean Aperture property. Specifies a rectangular crop within
/// the decoded image using rational arithmetic.
/// Note: clap is a plain box, NOT a FullBox.
/// </summary>
internal class ClapBox : Box
{
	public UInt32 CleanApertureWidthN { get; init; }
	public UInt32 CleanApertureWidthD { get; init; }
	public UInt32 CleanApertureHeightN { get; init; }
	public UInt32 CleanApertureHeightD { get; init; }
	public UInt32 HorizOffN { get; init; }
	public UInt32 HorizOffD { get; init; }
	public UInt32 VertOffN { get; init; }
	public UInt32 VertOffD { get; init; }

	public double CleanApertureWidth => CleanApertureWidthD != 0
		? (double)CleanApertureWidthN / CleanApertureWidthD : 0;
	public double CleanApertureHeight => CleanApertureHeightD != 0
		? (double)CleanApertureHeightN / CleanApertureHeightD : 0;

	public ClapBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, false)
	{
		CleanApertureWidthN = fr.ReadUInt32();
		CleanApertureWidthD = fr.ReadUInt32();
		CleanApertureHeightN = fr.ReadUInt32();
		CleanApertureHeightD = fr.ReadUInt32();
		HorizOffN = fr.ReadUInt32();
		HorizOffD = fr.ReadUInt32();
		VertOffN = fr.ReadUInt32();
		VertOffD = fr.ReadUInt32();
	}
}
