using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.Heif;

/// <summary>
/// pasp — Pixel Aspect Ratio property. Declares the sample aspect ratio.
/// Note: pasp is a plain box, NOT a FullBox.
/// </summary>
internal class PaspBox : Box
{
	public UInt32 HSpacing { get; init; }
	public UInt32 VSpacing { get; init; }

	public PaspBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, false)
	{
		HSpacing = fr.ReadUInt32();
		VSpacing = fr.ReadUInt32();
	}
}
