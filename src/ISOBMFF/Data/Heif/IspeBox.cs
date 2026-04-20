using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.Heif;

/// <summary>
/// ispe — Image Spatial Extents property. Declares the coded width and height
/// of an image item (before any transformative properties like irot/imir/clap).
/// </summary>
internal class IspeBox : Box
{
	public UInt32 ImageWidth { get; init; }
	public UInt32 ImageHeight { get; init; }

	public IspeBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, true)
	{
		ImageWidth = fr.ReadUInt32();
		ImageHeight = fr.ReadUInt32();
	}
}
