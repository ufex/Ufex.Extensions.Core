using System;
using Ufex.API;

namespace Ufex.Extensions.Core.ISOBMFF.Data.Heif;

/// <summary>
/// irot — Image Rotation property. Specifies a clockwise rotation in 90° increments.
/// Note: irot is a plain box, NOT a FullBox.
/// </summary>
internal class IrotBox : Box
{
	/// <summary>
	/// Rotation angle in 90° increments (0–3). Only the low 2 bits are significant.
	/// 0 = 0°, 1 = 90° CW, 2 = 180°, 3 = 270° CW.
	/// </summary>
	public Byte Angle { get; init; }

	public Int32 AngleDegrees => (Angle & 0x03) * 90;

	public IrotBox(FileReader fr, Int64 boxEndBoundary) : base(fr, boxEndBoundary, false)
	{
		Angle = fr.ReadByte();
	}
}
