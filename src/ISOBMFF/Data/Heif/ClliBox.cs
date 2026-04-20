using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.Heif;

/// <summary>
/// clli — Content Light Level Info property. HDR metadata with maximum content
/// light level and maximum frame-average light level.
/// </summary>
internal class ClliBox : Box
{
	/// <summary>
	/// Maximum Content Light Level in nits (cd/m²). 0 = unspecified.
	/// </summary>
	public UInt16 MaxContentLightLevel { get; init; }

	/// <summary>
	/// Maximum Frame-Average Light Level in nits. 0 = unspecified.
	/// </summary>
	public UInt16 MaxPicAverageLightLevel { get; init; }

	public ClliBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, false)
	{
		MaxContentLightLevel = fr.ReadUInt16();
		MaxPicAverageLightLevel = fr.ReadUInt16();
	}
}
